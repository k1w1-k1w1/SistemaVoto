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
    public class AdminAsignacionesController : ControllerBase
    {
        private readonly VotacionDbContext _context;

        public AdminAsignacionesController(VotacionDbContext context)
        {
            _context = context;
        }

        // =========================
        // ✅ LISTAR UBICACIONES (para el select del front)
        // GET: api/AdminAsignaciones/ubicaciones
        // =========================
        [HttpGet("ubicaciones")]
        public async Task<IActionResult> GetUbicaciones()
        {
            var ubicaciones = await _context.UbicacionesVotacion
                .OrderBy(u => u.Nombre)
                .ThenBy(u => u.NumeroMesa)
                .Select(u => new
                {
                    u.UbicacionId,
                    u.Nombre,
                    u.Direccion,
                    u.NumeroMesa,
                    u.CapacidadVotantes,
                    u.Activo
                })
                .ToListAsync();

            return Ok(ubicaciones);
        }

        // =========================
        // ✅ ASIGNAR VOTANTE A UBICACIÓN (POR CÉDULA)
        // POST: api/AdminAsignaciones/asignar-votante-cedula
        // body: { cedula, ubicacionId }
        // =========================
        [HttpPost("asignar-votante-cedula")]
        public async Task<IActionResult> AsignarVotantePorCedula([FromBody] AsignarVotantePorCedulaDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Cedula) || dto.UbicacionId <= 0)
                return BadRequest(new { mensaje = "Cedula y UbicacionId son requeridos." });

            var cedula = dto.Cedula.Trim();

            var usuario = await _context.Usuarios
                .Include(u => u.Votante)
                .FirstOrDefaultAsync(u => u.Cedula == cedula);

            if (usuario == null)
                return NotFound(new { mensaje = "No existe un usuario con esa cédula." });

            if (usuario.Rol != "Votante")
                return BadRequest(new { mensaje = "El usuario no tiene rol Votante." });

            var ubicacion = await _context.UbicacionesVotacion
                .FirstOrDefaultAsync(u => u.UbicacionId == dto.UbicacionId);

            if (ubicacion == null)
                return NotFound(new { mensaje = "Ubicación no encontrada." });

            if (!ubicacion.Activo)
                return BadRequest(new { mensaje = "La ubicación está inactiva." });

            // Si por alguna razón no existe el registro Votante, lo creamos
            if (usuario.Votante == null)
            {
                usuario.Votante = new Votante
                {
                    UsuarioId = usuario.UsuarioId,
                    Estado = "Registrado"
                };
                _context.Votantes.Add(usuario.Votante);
                await _context.SaveChangesAsync();
            }

            usuario.Votante.UbicacionAsignadaId = ubicacion.UbicacionId;
            if (string.IsNullOrWhiteSpace(usuario.Votante.Estado))
                usuario.Votante.Estado = "Registrado";

            _context.LogsActividad.Add(new LogActividad
            {
                UsuarioId = usuario.UsuarioId,
                Accion = "AsignarVotanteUbicacion",
                Descripcion = $"Votante asignado a UbicacionId={ubicacion.UbicacionId}",
                Fecha = DateTime.UtcNow,
                DatosAdicionales = $"Cedula={cedula}"
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "✅ Votante asignado correctamente.",
                votanteId = usuario.Votante.VotanteId,
                ubicacion = new { ubicacion.UbicacionId, ubicacion.Nombre, ubicacion.Direccion, ubicacion.NumeroMesa }
            });
        }

        // =========================
        // ✅ QUITAR VOTANTE DE UBICACIÓN (POR CÉDULA)
        // POST: api/AdminAsignaciones/quitar-votante-cedula
        // body: { cedula }
        // =========================
        [HttpPost("quitar-votante-cedula")]
        public async Task<IActionResult> QuitarVotantePorCedula([FromBody] QuitarVotantePorCedulaDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Cedula))
                return BadRequest(new { mensaje = "Cedula es requerida." });

            var cedula = dto.Cedula.Trim();

            var usuario = await _context.Usuarios
                .Include(u => u.Votante)
                .FirstOrDefaultAsync(u => u.Cedula == cedula);

            if (usuario == null || usuario.Votante == null)
                return NotFound(new { mensaje = "Votante no encontrado con esa cédula." });

            usuario.Votante.UbicacionAsignadaId = null;
            if (string.IsNullOrWhiteSpace(usuario.Votante.Estado))
                usuario.Votante.Estado = "Registrado";

            _context.LogsActividad.Add(new LogActividad
            {
                UsuarioId = usuario.UsuarioId,
                Accion = "QuitarVotanteUbicacion",
                Descripcion = "Se quitó la ubicación asignada del votante",
                Fecha = DateTime.UtcNow,
                DatosAdicionales = $"Cedula={cedula}"
            });

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "✅ Ubicación removida del votante correctamente." });
        }

        // =========================
        // ✅ ASIGNAR JEFE A UBICACIÓN (POR CÉDULA)
        // POST: api/AdminAsignaciones/asignar-jefe-cedula
        // body: { cedula, ubicacionId }
        // =========================
        [HttpPost("asignar-jefe-cedula")]
        public async Task<IActionResult> AsignarJefePorCedula([FromBody] AsignarJefePorCedulaDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Cedula) || dto.UbicacionId <= 0)
                return BadRequest(new { mensaje = "Cedula y UbicacionId son requeridos." });

            var cedula = dto.Cedula.Trim();

            var usuario = await _context.Usuarios
                .Include(u => u.JefeDeJunta)
                .FirstOrDefaultAsync(u => u.Cedula == cedula);

            if (usuario == null)
                return NotFound(new { mensaje = "No existe un usuario con esa cédula." });

            if (usuario.Rol != "JefeDeJunta")
                return BadRequest(new { mensaje = "El usuario no tiene rol JefeDeJunta." });

            var ubicacion = await _context.UbicacionesVotacion
                .FirstOrDefaultAsync(u => u.UbicacionId == dto.UbicacionId);

            if (ubicacion == null)
                return NotFound(new { mensaje = "Ubicación no encontrada." });

            if (!ubicacion.Activo)
                return BadRequest(new { mensaje = "La ubicación está inactiva." });

            if (usuario.JefeDeJunta == null)
            {
                usuario.JefeDeJunta = new JefeDeJunta
                {
                    UsuarioId = usuario.UsuarioId,
                    FechaAsignacion = DateTime.UtcNow
                };
                _context.JefesDeJunta.Add(usuario.JefeDeJunta);
                await _context.SaveChangesAsync();
            }

            usuario.JefeDeJunta.UbicacionAsignadaId = ubicacion.UbicacionId;
            usuario.JefeDeJunta.FechaAsignacion = DateTime.UtcNow;

            _context.LogsActividad.Add(new LogActividad
            {
                UsuarioId = usuario.UsuarioId,
                Accion = "AsignarJefeUbicacion",
                Descripcion = $"Jefe asignado a UbicacionId={ubicacion.UbicacionId}",
                Fecha = DateTime.UtcNow,
                DatosAdicionales = $"Cedula={cedula}"
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "✅ Jefe asignado correctamente.",
                jefeDeJuntaId = usuario.JefeDeJunta.JefeDeJuntaId,
                ubicacion = new { ubicacion.UbicacionId, ubicacion.Nombre, ubicacion.Direccion, ubicacion.NumeroMesa }
            });
        }

        // =========================
        // ✅ QUITAR JEFE DE UBICACIÓN (POR CÉDULA)
        // POST: api/AdminAsignaciones/quitar-jefe-cedula
        // body: { cedula }
        // =========================
        [HttpPost("quitar-jefe-cedula")]
        public async Task<IActionResult> QuitarJefePorCedula([FromBody] QuitarJefePorCedulaDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Cedula))
                return BadRequest(new { mensaje = "Cedula es requerida." });

            var cedula = dto.Cedula.Trim();

            var usuario = await _context.Usuarios
                .Include(u => u.JefeDeJunta)
                .FirstOrDefaultAsync(u => u.Cedula == cedula);

            if (usuario == null || usuario.JefeDeJunta == null)
                return NotFound(new { mensaje = "Jefe de Junta no encontrado con esa cédula." });

            usuario.JefeDeJunta.UbicacionAsignadaId = null;

            _context.LogsActividad.Add(new LogActividad
            {
                UsuarioId = usuario.UsuarioId,
                Accion = "QuitarJefeUbicacion",
                Descripcion = "Se quitó la ubicación asignada del jefe de junta",
                Fecha = DateTime.UtcNow,
                DatosAdicionales = $"Cedula={cedula}"
            });

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "✅ Ubicación removida del jefe correctamente." });
        }
    }
}
