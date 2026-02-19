using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVoto.API.Data;
using SistemaVoto.API.Services;
using SistemaVoto.Models;
using System.Security.Claims;
using System;
using System.Linq;


namespace SistemaVoto.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "JefeDeJunta")]
    public class JefeDeJuntaController : ControllerBase
    {
        private readonly VotacionDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ISMSService _smsService;
        private readonly ILogger<JefeDeJuntaController> _logger;

        public JefeDeJuntaController(
            VotacionDbContext context,
            IEmailService emailService,
            ISMSService smsService,
            ILogger<JefeDeJuntaController> logger)
        {
            _context = context;
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
        }

        // Helpers
        private int? GetUsuarioIdFromToken()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out var id) ? id : null;
        }

        private async Task<JefeDeJunta?> GetJefeActualAsync()
        {
            var usuarioId = GetUsuarioIdFromToken();
            if (!usuarioId.HasValue) return null;

            return await _context.JefesDeJunta
                .Include(j => j.UbicacionAsignada)
                    .ThenInclude(u => u.VotantesAsignados)
                .FirstOrDefaultAsync(j => j.UsuarioId == usuarioId.Value);
        }

        private static string GenerarCodigoUnico()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // 1) Estadísticas del recinto del jefe logueado (sin jefeId)
        // GET: api/JefeDeJunta/estadisticas-mi-recinto
        [HttpGet("estadisticas-mi-recinto")]
        public async Task<ActionResult<object>> ObtenerEstadisticasMiRecinto()
        {
            var jefe = await GetJefeActualAsync();
            if (jefe == null)
                return Unauthorized(new { mensaje = "No se pudo identificar al Jefe de Junta desde el token." });

            if (jefe.UbicacionAsignadaId == null)
                return BadRequest(new { mensaje = "El Jefe de Junta no tiene ubicación asignada." });


            var ubicacion = jefe.UbicacionAsignada;

            var totalVotantes = ubicacion.VotantesAsignados.Count;
            var votantesPresentes = ubicacion.VotantesAsignados.Count(v => v.Estado == "PresenteEnRecinto" || v.Estado == "VotoEmitido");
            var votosEmitidos = ubicacion.VotantesAsignados.Count(v => v.Estado == "VotoEmitido");

            var codigosGeneradosHoy = await _context.CodigosVoto.CountAsync(c =>
                c.JefeDeJuntaId == jefe.JefeDeJuntaId &&
                c.FechaGeneracion.Date == DateTime.UtcNow.Date);

            return Ok(new
            {
                jefe = new
                {
                    jefeDeJuntaId = jefe.JefeDeJuntaId,
                    jefe.UsuarioId
                },
                ubicacion = new
                {
                    ubicacion.UbicacionId,
                    ubicacion.Nombre,
                    ubicacion.Direccion,
                    ubicacion.NumeroMesa
                },
                estadisticas = new
                {
                    totalVotantesAsignados = totalVotantes,
                    votantesPresentes,
                    votosEmitidos,
                    codigosGeneradosHoy,
                    porcentajeParticipacion = totalVotantes > 0
                        ? Math.Round((votosEmitidos * 100.0 / totalVotantes), 2)
                        : 0
                }
            });
        }

        // 2) Códigos generados HOY por el jefe logueado (sin jefeId)
        // GET: api/JefeDeJunta/codigos-hoy
        [HttpGet("codigos-hoy")]
        public async Task<ActionResult<object>> ObtenerCodigosHoy([FromQuery] int take = 20)
        {
            var jefe = await GetJefeActualAsync();
            if (jefe == null)
                return Unauthorized(new { mensaje = "No se pudo identificar al Jefe de Junta desde el token." });

            take = Math.Clamp(take, 1, 100);

            var hoy = DateTime.UtcNow.Date;

            var codigos = await _context.CodigosVoto
                .Include(c => c.Votante)
                    .ThenInclude(v => v.Usuario)
                .Include(c => c.Eleccion)
                .Where(c => c.JefeDeJuntaId == jefe.JefeDeJuntaId && c.FechaGeneracion.Date == hoy)
                .OrderByDescending(c => c.FechaGeneracion)
                .Take(take)
                .Select(c => new
                {
                    c.CodigoVotoId,
                    c.Codigo,
                    c.Estado,
                    c.FechaGeneracion,
                    votante = new
                    {
                        c.VotanteId,
                        email = c.Votante.Usuario.Email,
                        cedula = c.Votante.Usuario.Cedula,
                        nombreCompleto = c.Votante.Usuario.Nombre + " " + c.Votante.Usuario.Apellido
                    },
                    eleccion = new
                    {
                        c.EleccionId,
                        c.Eleccion.Nombre,
                        c.Eleccion.Tipo
                    }
                })
                .ToListAsync();

            return Ok(new
            {
                total = codigos.Count,
                codigos
            });
        }

        // 3) Verificar votante (sin jefeId)
        // POST: api/JefeDeJunta/verificar-votante
        // body: { cedula, eleccionId }
        [HttpPost("verificar-votante")]
        public async Task<ActionResult<object>> VerificarVotante(VerificarVotanteDto dto)
        {
            var jefe = await GetJefeActualAsync();
            if (jefe == null)
                return Unauthorized(new { mensaje = "No se pudo identificar al Jefe de Junta desde el token." });

            if (jefe.UbicacionAsignadaId == null)
                return BadRequest(new { mensaje = "El Jefe de Junta no tiene ubicación asignada." });

            var usuario = await _context.Usuarios
                .Include(u => u.Votante)
                    .ThenInclude(v => v.UbicacionAsignada)
                .FirstOrDefaultAsync(u => u.Cedula == dto.Cedula);

            if (usuario == null || usuario.Votante == null)
            {
                return NotFound(new
                {
                    mensaje = "Votante no encontrado",
                    puedeVotar = false,
                    motivo = "La cédula no está registrada en el sistema"
                });
            }



            var votante = usuario.Votante;

            if (votante.UbicacionAsignadaId == null)
            {
                return BadRequest(new
                {
                    mensaje = "Votante sin ubicación asignada",
                    puedeVotar = false,
                    motivo = "El votante no tiene mesa electoral asignada"
                });
            }

            if (votante.UbicacionAsignadaId != jefe.UbicacionAsignadaId)
            {
                return BadRequest(new
                {
                    mensaje = "Votante no pertenece a tu recinto",
                    puedeVotar = false,
                    motivo = "No puedes verificar votantes de otra ubicación"
                });
            }

            


            // Verificar si ya votó en esta elección
            var yaVoto = await _context.Votos
                .AnyAsync(v => v.VotanteId == votante.VotanteId && v.EleccionId == dto.EleccionId);

            if (yaVoto)
            {
                return BadRequest(new
                {
                    mensaje = "Votante ya emitió su voto",
                    puedeVotar = false,
                    motivo = "Este votante ya ejerció su derecho al voto en esta elección"
                });
            }

            // Verificar si ya tiene un código generado para esta elección
            var codigoExistente = await _context.CodigosVoto
                .FirstOrDefaultAsync(c => c.VotanteId == votante.VotanteId
                    && c.EleccionId == dto.EleccionId
                    && c.Estado == "Generado");

            return Ok(new
            {
                mensaje = "Votante verificado exitosamente",
                puedeVotar = true,
                votante = new
                {
                    votanteId = votante.VotanteId,
                    nombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
                    cedula = usuario.Cedula,
                    email = usuario.Email,
                    telefono = usuario.Telefono,
                    ubicacion = votante.UbicacionAsignada?.Nombre,
                    mesa = votante.UbicacionAsignada?.NumeroMesa,
                    estado = votante.Estado,
                    tieneCodigoGenerado = codigoExistente != null,
                    codigoExistente = codigoExistente?.Codigo
                }
            });
        }

        // 4) Generar código (SIN JefeDeJuntaId)
        // POST: api/JefeDeJunta/generar-codigo
        // body: { votanteId, eleccionId }
        [HttpPost("generar-codigo")]
        public async Task<ActionResult<object>> GenerarCodigo(GenerarCodigoSinJefeDto dto)
        {
            var jefe = await GetJefeActualAsync();
            if (jefe == null)
                return Unauthorized(new { mensaje = "No se pudo identificar al Jefe de Junta desde el token." });

            if (jefe.UbicacionAsignadaId == null)
                return BadRequest(new { mensaje = "El Jefe de Junta no tiene ubicación asignada." });

            // Verificar votante
            var votante = await _context.Votantes
                .Include(v => v.Usuario)
                .FirstOrDefaultAsync(v => v.VotanteId == dto.VotanteId);

            if (votante == null)
                return NotFound(new { mensaje = "Votante no encontrado" });

            // (Opcional) Validar que el votante pertenezca a la ubicación del jefe
            if (votante.UbicacionAsignadaId != jefe.UbicacionAsignadaId)
            {
                return BadRequest(new
                {
                    mensaje = "Votante no pertenece a tu recinto",
                    motivo = "No puedes generar códigos a votantes de otra ubicación"
                });
            }

            // Verificar elección
            var eleccion = await _context.Elecciones.FindAsync(dto.EleccionId);
            if (eleccion == null)
                return NotFound(new { mensaje = "Elección no encontrada" });

            if (eleccion.Estado != "Activa")
                return BadRequest(new { mensaje = $"La elección no está activa. Estado: {eleccion.Estado}" });

            // Si ya existe código, reenviar (si está Generado o incluso si existe cualquiera)
            var codigoExistente = await _context.CodigosVoto
                .FirstOrDefaultAsync(c => c.VotanteId == dto.VotanteId
                && c.EleccionId == dto.EleccionId
                && c.Estado == "Generado");

            if (codigoExistente != null)
            {
                bool emailEnviado = false;
                bool smsEnviado = false;

                if (!string.IsNullOrWhiteSpace(votante.Usuario?.Email))
                {
                    emailEnviado = await _emailService.EnviarCodigoVotoAsync(
                        votante.Usuario.Email,
                        $"{votante.Usuario.Nombre} {votante.Usuario.Apellido}",
                        codigoExistente.Codigo
                    );
                }

                if (!string.IsNullOrWhiteSpace(votante.Usuario?.Telefono))
                {
                    smsEnviado = await _smsService.EnviarCodigoVotoAsync(
                        votante.Usuario.Telefono,
                        $"{votante.Usuario.Nombre} {votante.Usuario.Apellido}",
                        codigoExistente.Codigo
                    );
                }

                return Ok(new
                {
                    mensaje = "Código reenviado exitosamente",
                    codigoVotoId = codigoExistente.CodigoVotoId,
                    codigo = codigoExistente.Codigo,
                    emailEnviado,
                    smsEnviado,
                    yaExistia = true,
                    destinatarios = new
                    {
                        email = votante.Usuario?.Email,
                        telefono = votante.Usuario?.Telefono
                    }
                });
            }

            // Generar nuevo código único
            string nuevoCodigo;
            bool codigoUnico;
            do
            {
                nuevoCodigo = GenerarCodigoUnico();
                codigoUnico = !await _context.CodigosVoto.AnyAsync(c => c.Codigo == nuevoCodigo);
            } while (!codigoUnico);

            var codigoVoto = new CodigoVoto
            {
                VotanteId = dto.VotanteId,
                EleccionId = dto.EleccionId,
                JefeDeJuntaId = jefe.JefeDeJuntaId,
                Codigo = nuevoCodigo,
                FechaGeneracion = DateTime.UtcNow,
                Estado = "Generado"
            };

            _context.CodigosVoto.Add(codigoVoto);

            // Marcar al votante como presente
            votante.Estado = "PresenteEnRecinto";
            votante.FechaPresenciaRecinto = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Enviar email y sms
            bool emailEnviadoExito = false;
            bool smsEnviadoExito = false;

            if (!string.IsNullOrWhiteSpace(votante.Usuario?.Email))
            {
                emailEnviadoExito = await _emailService.EnviarCodigoVotoAsync(
                    votante.Usuario.Email,
                    $"{votante.Usuario.Nombre} {votante.Usuario.Apellido}",
                    nuevoCodigo
                );
            }

            if (!string.IsNullOrWhiteSpace(votante.Usuario?.Telefono))
            {
                smsEnviadoExito = await _smsService.EnviarCodigoVotoAsync(
                    votante.Usuario.Telefono,
                    $"{votante.Usuario.Nombre} {votante.Usuario.Apellido}",
                    nuevoCodigo
                );
            }

            // Registrar log (DatosAdicionales NO NULL)
            var log = new LogActividad
            {
                UsuarioId = jefe.UsuarioId,
                Accion = "GeneracionCodigo",
                Descripcion = $"Código generado para VotanteId {dto.VotanteId} en EleccionId {dto.EleccionId}",
                Fecha = DateTime.UtcNow,
                DatosAdicionales = $"CodigoVotoId={codigoVoto.CodigoVotoId};Codigo={nuevoCodigo}"
            };

            _context.LogsActividad.Add(log);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Código generado y enviado exitosamente",
                codigoVotoId = codigoVoto.CodigoVotoId,
                codigo = nuevoCodigo,
                emailEnviado = emailEnviadoExito,
                smsEnviado = smsEnviadoExito,
                yaExistia = false,
                destinatarios = new
                {
                    email = votante.Usuario?.Email,
                    telefono = votante.Usuario?.Telefono
                }
            });
        }

        // 5) Reportar incidencia (sin jefeId desde front si quieres)
        // POST: api/JefeDeJunta/reportar-incidencia
        // body: { votanteId?, tipo, descripcion }
        [HttpPost("reportar-incidencia")]
        public async Task<ActionResult<object>> ReportarIncidencia(ReportarIncidenciaSinJefeDto dto)
        {
            var jefe = await GetJefeActualAsync();
            if (jefe == null)
                return Unauthorized(new { mensaje = "No se pudo identificar al Jefe de Junta desde el token." });

            var incidencia = new Incidencia
            {
                JefeDeJuntaId = jefe.JefeDeJuntaId,
                VotanteId = dto.VotanteId,
                Tipo = dto.Tipo,
                Descripcion = dto.Descripcion,
                FechaReporte = DateTime.UtcNow,
                Estado = "Pendiente"
            };

            _context.Incidencias.Add(incidencia);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Incidencia reportada exitosamente",
                incidenciaId = incidencia.IncidenciaId,
                estado = incidencia.Estado
            });
        }

        [HttpGet("whoami")]
        public IActionResult WhoAmI()
        {
            return Ok(new
            {
                nameId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                email = User.FindFirstValue(ClaimTypes.Email),
                name = User.FindFirstValue(ClaimTypes.Name),
                role = User.FindFirstValue(ClaimTypes.Role),
                cedula = User.FindFirstValue("cedula")
            });
        }


    }

    // DTOs
    public class VerificarVotanteDto
    {
        public string Cedula { get; set; }
        public int EleccionId { get; set; }
    }

    public class GenerarCodigoSinJefeDto
    {
        public int VotanteId { get; set; }
        public int EleccionId { get; set; }
    }

    public class ReportarIncidenciaSinJefeDto
    {
        public int? VotanteId { get; set; }
        public string Tipo { get; set; }
        public string Descripcion { get; set; }
    }
}
