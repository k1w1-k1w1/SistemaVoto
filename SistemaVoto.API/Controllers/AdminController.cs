using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVoto.API.Data;
using SistemaVoto.API.DTOs;
using SistemaVoto.Models;

namespace SistemaVoto.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class AdminController : ControllerBase
    {
        private readonly VotacionDbContext _context;

        public AdminController(VotacionDbContext context)
        {
            _context = context;
        }

        // =========================
        // DASHBOARD
        // =========================
        [HttpGet("dashboard")]
        public async Task<ActionResult<object>> GetDashboard()
        {
            var totalUsuarios = await _context.Usuarios.CountAsync();
            var totalVotantes = await _context.Votantes.CountAsync();
            var totalJefes = await _context.JefesDeJunta.CountAsync();
            var totalElecciones = await _context.Elecciones.CountAsync();
            var eleccionesActivas = await _context.Elecciones.CountAsync(e => e.Estado == "Activa");
            var totalVotos = await _context.Votos.CountAsync();
            var totalUbicaciones = await _context.UbicacionesVotacion.CountAsync();
            var totalIncidencias = await _context.Incidencias.CountAsync();
            var incidenciasPendientes = await _context.Incidencias.CountAsync(i => i.Estado == "Pendiente");

            var ultimosLogs = await _context.LogsActividad
                .Include(l => l.Usuario)
                .OrderByDescending(l => l.Fecha)
                .Take(10)
                .Select(l => new
                {
                    l.LogId,
                    l.Accion,
                    l.Descripcion,
                    l.Fecha,
                    Usuario = l.Usuario != null ? l.Usuario.Nombre + " " + l.Usuario.Apellido : "Sistema"
                })
                .ToListAsync();

            return Ok(new
            {
                resumen = new
                {
                    totalUsuarios,
                    totalVotantes,
                    totalJefes,
                    totalElecciones,
                    eleccionesActivas,
                    totalVotos,
                    totalUbicaciones,
                    totalIncidencias,
                    incidenciasPendientes
                },
                ultimaActividad = ultimosLogs
            });
        }

        // =========================
        // USUARIOS
        // =========================
        [HttpGet("usuarios")]
        public async Task<ActionResult<IEnumerable<object>>> GetUsuarios(
            [FromQuery] string? rol = null,
            [FromQuery] string? estado = null,
            [FromQuery] string? buscar = null)
        {
            var query = _context.Usuarios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(rol))
                query = query.Where(u => u.Rol == rol);

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(u => u.Estado == estado);

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                query = query.Where(u =>
                    u.Nombre.Contains(buscar) ||
                    u.Apellido.Contains(buscar) ||
                    u.Cedula.Contains(buscar) ||
                    u.Email.Contains(buscar));
            }

            var usuarios = await query
                .OrderByDescending(u => u.FechaCreacion)
                .Select(u => new
                {
                    u.UsuarioId,
                    u.Nombre,
                    u.Apellido,
                    u.Cedula,
                    u.Email,
                    u.Telefono,
                    u.Rol,
                    u.Estado,
                    u.FechaCreacion,
                    u.FechaVerificacion
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        [HttpPut("usuarios/{id}/cambiar-rol")]
        public async Task<IActionResult> CambiarRolUsuario(int id, [FromBody] CambiarRolDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.NuevoRol))
                return BadRequest(new { mensaje = "NuevoRol es requerido." });

            var usuario = await _context.Usuarios
                .Include(u => u.Votante)
                .Include(u => u.JefeDeJunta)
                .FirstOrDefaultAsync(u => u.UsuarioId == id);

            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            if (dto.NuevoRol != "Votante" && dto.NuevoRol != "JefeDeJunta" && dto.NuevoRol != "Administrador")
                return BadRequest(new { mensaje = "Rol inválido. Use: Votante, JefeDeJunta o Administrador" });

            var rolAnterior = usuario.Rol;

            // Votante -> JefeDeJunta
            if (dto.NuevoRol == "JefeDeJunta" && usuario.Votante != null)
            {
                _context.Votantes.Remove(usuario.Votante);

                var jefe = new JefeDeJunta
                {
                    UsuarioId = usuario.UsuarioId,
                    FechaAsignacion = DateTime.UtcNow
                };
                _context.JefesDeJunta.Add(jefe);
            }

            // JefeDeJunta -> Votante
            if (dto.NuevoRol == "Votante" && usuario.JefeDeJunta != null)
            {
                _context.JefesDeJunta.Remove(usuario.JefeDeJunta);

                var votante = new Votante
                {
                    UsuarioId = usuario.UsuarioId,
                    Estado = "Registrado"
                };
                _context.Votantes.Add(votante);
            }

            usuario.Rol = dto.NuevoRol;
            await _context.SaveChangesAsync();

            _context.LogsActividad.Add(new LogActividad
            {
                UsuarioId = id,
                Accion = "CambioRol",
                Descripcion = $"Rol cambiado de {rolAnterior} a {dto.NuevoRol}",
                Fecha = DateTime.UtcNow,
                DatosAdicionales = ""
            });
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Rol actualizado exitosamente",
                rolAnterior,
                nuevoRol = dto.NuevoRol
            });
        }

        [HttpPut("usuarios/{id}/cambiar-estado")]
        public async Task<IActionResult> CambiarEstadoUsuario(int id, [FromBody] CambiarEstadoDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.NuevoEstado))
                return BadRequest(new { mensaje = "NuevoEstado es requerido." });

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            if (dto.NuevoEstado != "PendienteVerificacion" && dto.NuevoEstado != "Activa" &&
                dto.NuevoEstado != "Bloqueada" && dto.NuevoEstado != "Suspendida")
            {
                return BadRequest(new { mensaje = "Estado inválido. Use: PendienteVerificacion, Activa, Bloqueada o Suspendida" });
            }

            var estadoAnterior = usuario.Estado;
            usuario.Estado = dto.NuevoEstado;

            if (dto.NuevoEstado == "Activa" && usuario.FechaVerificacion == null)
                usuario.FechaVerificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _context.LogsActividad.Add(new LogActividad
            {
                UsuarioId = id,
                Accion = "CambioEstado",
                Descripcion = $"Estado cambiado de {estadoAnterior} a {dto.NuevoEstado}",
                Fecha = DateTime.UtcNow,
                DatosAdicionales = ""
            });
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Estado actualizado exitosamente",
                estadoAnterior,
                nuevoEstado = dto.NuevoEstado
            });
        }

        // =========================
        // ASIGNAR / QUITAR VOTANTE (DTO NUEVO: por ID)
        // =========================
        // POST: api/Admin/asignar-votante
        // body: { votanteId, ubicacionId }
        [HttpPost("asignar-votante")]
        public async Task<IActionResult> AsignarVotante([FromBody] AsignarVotanteDto dto)
        {
            if (dto == null || dto.VotanteId <= 0 || dto.UbicacionId <= 0)
                return BadRequest(new { mensaje = "VotanteId y UbicacionId son requeridos." });

            var ubicacion = await _context.UbicacionesVotacion.FindAsync(dto.UbicacionId);
            if (ubicacion == null)
                return NotFound(new { mensaje = "Ubicación no encontrada." });

            var votante = await _context.Votantes
                .Include(v => v.Usuario)
                .FirstOrDefaultAsync(v => v.VotanteId == dto.VotanteId);

            if (votante == null)
                return NotFound(new { mensaje = "Votante no encontrado." });

            // seguridad: verificar rol del usuario dueño del votante
            if (votante.Usuario == null || votante.Usuario.Rol != "Votante")
                return BadRequest(new { mensaje = "El usuario asociado no tiene rol Votante." });

            votante.UbicacionAsignadaId = dto.UbicacionId;
            await _context.SaveChangesAsync();

            _context.LogsActividad.Add(new LogActividad
            {
                UsuarioId = votante.UsuarioId,
                Accion = "AsignarVotanteUbicacion",
                Descripcion = $"VotanteId={dto.VotanteId} asignado a UbicacionId={dto.UbicacionId}",
                Fecha = DateTime.UtcNow,
                DatosAdicionales = ""
            });
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Votante asignado exitosamente",
                votanteId = votante.VotanteId,
                ubicacionId = dto.UbicacionId
            });
        }

        // POST: api/Admin/quitar-votante
        // body: { cedula }
        [HttpPost("quitar-votante")]
        public async Task<IActionResult> QuitarVotante([FromBody] QuitarVotanteDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Cedula))
                return BadRequest(new { mensaje = "Cedula es requerida." });

            var usuario = await _context.Usuarios
                .Include(u => u.Votante)
                .FirstOrDefaultAsync(u => u.Cedula == dto.Cedula);

            if (usuario == null || usuario.Votante == null)
                return NotFound(new { mensaje = "Votante no encontrado con esa cédula." });

            usuario.Votante.UbicacionAsignadaId = null;
            await _context.SaveChangesAsync();

            _context.LogsActividad.Add(new LogActividad
            {
                UsuarioId = usuario.UsuarioId,
                Accion = "QuitarVotanteUbicacion",
                Descripcion = $"Se quitó la ubicación del votante (Cedula={dto.Cedula})",
                Fecha = DateTime.UtcNow,
                DatosAdicionales = ""
            });
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Ubicación del votante removida exitosamente" });
        }

        // =========================
        // ASIGNAR / QUITAR JEFE (DTO NUEVO: por ID)
        // =========================
        // POST: api/Admin/asignar-jefe
        // body: { jefeDeJuntaId, ubicacionId }
        [HttpPost("asignar-jefe")]
        public async Task<IActionResult> AsignarJefe([FromBody] AsignarJefeDto dto)
        {
            if (dto == null || dto.JefeDeJuntaId <= 0 || dto.UbicacionId <= 0)
                return BadRequest(new { mensaje = "JefeDeJuntaId y UbicacionId son requeridos." });

            var ubicacion = await _context.UbicacionesVotacion.FindAsync(dto.UbicacionId);
            if (ubicacion == null)
                return NotFound(new { mensaje = "Ubicación no encontrada." });

            var jefe = await _context.JefesDeJunta
                .Include(j => j.Usuario)
                .FirstOrDefaultAsync(j => j.JefeDeJuntaId == dto.JefeDeJuntaId);

            if (jefe == null)
                return NotFound(new { mensaje = "Jefe de Junta no encontrado." });

            if (jefe.Usuario == null || jefe.Usuario.Rol != "JefeDeJunta")
                return BadRequest(new { mensaje = "El usuario asociado no tiene rol JefeDeJunta." });

            jefe.UbicacionAsignadaId = dto.UbicacionId;
            jefe.FechaAsignacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _context.LogsActividad.Add(new LogActividad
            {
                UsuarioId = jefe.UsuarioId,
                Accion = "AsignarJefeUbicacion",
                Descripcion = $"JefeDeJuntaId={dto.JefeDeJuntaId} asignado a UbicacionId={dto.UbicacionId}",
                Fecha = DateTime.UtcNow,
                DatosAdicionales = ""
            });
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Jefe de Junta asignado exitosamente",
                jefeDeJuntaId = jefe.JefeDeJuntaId,
                ubicacionId = dto.UbicacionId
            });
        }

        // POST: api/Admin/quitar-jefe
        // body: { cedula }
        [HttpPost("quitar-jefe")]
        public async Task<IActionResult> QuitarJefe([FromBody] QuitarJefeDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Cedula))
                return BadRequest(new { mensaje = "Cedula es requerida." });

            var usuario = await _context.Usuarios
                .Include(u => u.JefeDeJunta)
                .FirstOrDefaultAsync(u => u.Cedula == dto.Cedula);

            if (usuario == null || usuario.JefeDeJunta == null)
                return NotFound(new { mensaje = "Jefe de Junta no encontrado con esa cédula." });

            usuario.JefeDeJunta.UbicacionAsignadaId = null;
            await _context.SaveChangesAsync();

            _context.LogsActividad.Add(new LogActividad
            {
                UsuarioId = usuario.UsuarioId,
                Accion = "QuitarJefeUbicacion",
                Descripcion = $"Se quitó la ubicación del jefe (Cedula={dto.Cedula})",
                Fecha = DateTime.UtcNow,
                DatosAdicionales = ""
            });
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Ubicación del jefe removida exitosamente" });
        }

        // =========================
        // LOGS
        // =========================
        [HttpGet("logs")]
        public async Task<ActionResult<object>> GetLogs(
            [FromQuery] string? accion = null,
            [FromQuery] int? usuarioId = null,
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int cantidad = 20)
        {
            var query = _context.LogsActividad
                .Include(l => l.Usuario)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(accion))
                query = query.Where(l => l.Accion == accion);

            if (usuarioId.HasValue)
                query = query.Where(l => l.UsuarioId == usuarioId);

            if (desde.HasValue)
                query = query.Where(l => l.Fecha >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(l => l.Fecha <= hasta.Value);

            var total = await query.CountAsync();

            var logs = await query
                .OrderByDescending(l => l.Fecha)
                .Skip((pagina - 1) * cantidad)
                .Take(cantidad)
                .Select(l => new
                {
                    l.LogId,
                    l.Accion,
                    l.Descripcion,
                    l.DireccionIP,
                    l.Fecha,
                    l.DatosAdicionales,
                    Usuario = l.Usuario != null ? new
                    {
                        l.Usuario.UsuarioId,
                        NombreCompleto = l.Usuario.Nombre + " " + l.Usuario.Apellido,
                        l.Usuario.Email,
                        l.Usuario.Rol
                    } : null
                })
                .ToListAsync();

            return Ok(new
            {
                total,
                pagina,
                cantidad,
                totalPaginas = (int)Math.Ceiling(total / (double)cantidad),
                logs
            });
        }

        // =========================
        // INCIDENCIAS
        // =========================
        [HttpGet("incidencias")]
        public async Task<ActionResult<IEnumerable<object>>> GetIncidencias(
            [FromQuery] string? estado = null,
            [FromQuery] string? tipo = null)
        {
            var query = _context.Incidencias
                .Include(i => i.JefeDeJunta).ThenInclude(j => j.Usuario)
                .Include(i => i.Votante).ThenInclude(v => v.Usuario)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(i => i.Estado == estado);

            if (!string.IsNullOrWhiteSpace(tipo))
                query = query.Where(i => i.Tipo == tipo);

            var incidencias = await query
                .OrderByDescending(i => i.FechaReporte)
                .Select(i => new
                {
                    i.IncidenciaId,
                    i.Tipo,
                    i.Descripcion,
                    i.FechaReporte,
                    i.Estado,
                    i.Resolucion,
                    JefeDeJunta = new
                    {
                        i.JefeDeJunta.JefeDeJuntaId,
                        NombreCompleto = i.JefeDeJunta.Usuario.Nombre + " " + i.JefeDeJunta.Usuario.Apellido
                    },
                    Votante = i.Votante != null ? new
                    {
                        i.Votante.VotanteId,
                        NombreCompleto = i.Votante.Usuario.Nombre + " " + i.Votante.Usuario.Apellido,
                        i.Votante.Usuario.Cedula
                    } : null
                })
                .ToListAsync();

            return Ok(incidencias);
        }

        [HttpPut("incidencias/{id}/resolver")]
        public async Task<IActionResult> ResolverIncidencia(int id, [FromBody] ResolverIncidenciaDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Estado))
                return BadRequest(new { mensaje = "Estado es requerido." });

            if (dto.Estado != "Resuelta" && dto.Estado != "Rechazada")
                return BadRequest(new { mensaje = "Estado inválido. Use: Resuelta o Rechazada" });

            var incidencia = await _context.Incidencias.FindAsync(id);
            if (incidencia == null)
                return NotFound(new { mensaje = "Incidencia no encontrada" });

            if (incidencia.Estado == "Resuelta" || incidencia.Estado == "Rechazada")
                return BadRequest(new { mensaje = $"La incidencia ya fue {incidencia.Estado.ToLower()}" });

            incidencia.Estado = dto.Estado;
            incidencia.Resolucion = dto.Resolucion;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = $"Incidencia {dto.Estado.ToLower()} exitosamente",
                incidenciaId = incidencia.IncidenciaId,
                estado = incidencia.Estado,
                resolucion = incidencia.Resolucion
            });
        }

        // =========================
        // ESTADÍSTICAS GENERALES
        // =========================
        [HttpGet("estadisticas-generales")]
        public async Task<ActionResult<object>> GetEstadisticasGenerales()
        {
            var elecciones = await _context.Elecciones
                .Include(e => e.Votos)
                .Include(e => e.Candidatos)
                .ToListAsync();

            var estadisticasPorEleccion = elecciones.Select(e => new
            {
                e.EleccionId,
                e.Nombre,
                e.Tipo,
                e.Estado,
                TotalCandidatos = e.Candidatos.Count,
                TotalVotos = e.Votos.Count,
                VotosPorTipo = new
                {
                    Individual = e.Votos.Count(v => v.TipoVoto == "Individual"),
                    Plancha = e.Votos.Count(v => v.TipoVoto == "Plancha"),
                    Blanco = e.Votos.Count(v => v.TipoVoto == "Blanco")
                }
            }).ToList();

            var ubicaciones = await _context.UbicacionesVotacion
                .Include(u => u.VotantesAsignados)
                .Select(u => new
                {
                    u.UbicacionId,
                    u.Nombre,
                    u.NumeroMesa,
                    TotalVotantes = u.VotantesAsignados.Count,
                    VotosEmitidos = u.VotantesAsignados.Count(v => v.Estado == "VotoEmitido"),
                    Porcentaje = u.VotantesAsignados.Count > 0
                        ? Math.Round((u.VotantesAsignados.Count(v => v.Estado == "VotoEmitido") * 100.0 / u.VotantesAsignados.Count), 2)
                        : 0
                })
                .ToListAsync();

            return Ok(new
            {
                elecciones = estadisticasPorEleccion,
                ubicaciones,
                resumenGeneral = new
                {
                    totalUsuariosRegistrados = await _context.Usuarios.CountAsync(),
                    totalVotantesRegistrados = await _context.Votantes.CountAsync(),
                    totalVotosEmitidos = await _context.Votos.CountAsync(),
                    totalIncidencias = await _context.Incidencias.CountAsync(),
                    incidenciasPendientes = await _context.Incidencias.CountAsync(i => i.Estado == "Pendiente")
                }
            });
        }

        // =========================
        // EXPORTAR RESULTADOS
        // =========================
        [HttpGet("exportar-resultados/{eleccionId}")]
        public async Task<ActionResult<object>> ExportarResultados(int eleccionId)
        {
            var eleccion = await _context.Elecciones
                .Include(e => e.Candidatos).ThenInclude(c => c.Votos)
                .Include(e => e.Candidatos).ThenInclude(c => c.ListaPolitica)
                .FirstOrDefaultAsync(e => e.EleccionId == eleccionId);

            if (eleccion == null)
                return NotFound(new { mensaje = "Elección no encontrada" });

            var totalVotos = await _context.Votos.CountAsync(v => v.EleccionId == eleccionId);

            var votosBlanco = await _context.Votos.CountAsync(v =>
                v.EleccionId == eleccionId && v.TipoVoto == "Blanco");

            var votosPlancha = await _context.Votos.CountAsync(v =>
                v.EleccionId == eleccionId && v.TipoVoto == "Plancha");

            var votosIndividual = await _context.Votos.CountAsync(v =>
                v.EleccionId == eleccionId && v.TipoVoto == "Individual");

            var resultadosCandidatos = eleccion.Candidatos
                .Select(c => new
                {
                    c.CandidatoId,
                    NombreCompleto = $"{c.Nombre} {c.Apellido}",
                    c.Cedula,
                    ListaPolitica = c.ListaPolitica?.Nombre,
                    Siglas = c.ListaPolitica?.Siglas,
                    VotosDirectos = c.Votos.Count(v => v.TipoVoto == "Individual"),
                    VotosPlancha = c.ListaPoliticaId.HasValue
                        ? _context.Votos.Count(v => v.EleccionId == eleccionId
                            && v.TipoVoto == "Plancha"
                            && v.ListaPoliticaId == c.ListaPoliticaId)
                        : 0,
                    TotalVotos = c.Votos.Count(v => v.TipoVoto == "Individual") +
                                 (c.ListaPoliticaId.HasValue
                                     ? _context.Votos.Count(v => v.EleccionId == eleccionId
                                         && v.TipoVoto == "Plancha"
                                         && v.ListaPoliticaId == c.ListaPoliticaId)
                                     : 0)
                })
                .OrderByDescending(r => r.TotalVotos)
                .ToList();

            return Ok(new
            {
                eleccion = new
                {
                    eleccion.EleccionId,
                    eleccion.Nombre,
                    eleccion.Tipo,
                    eleccion.Estado,
                    eleccion.FechaInicio,
                    eleccion.FechaFin
                },
                resumenVotos = new
                {
                    totalVotos,
                    votosIndividual,
                    votosPlancha,
                    votosBlanco
                },
                resultados = resultadosCandidatos,
                ganador = resultadosCandidatos.FirstOrDefault(),
                fechaExportacion = DateTime.UtcNow
            });
        }
    }
}
