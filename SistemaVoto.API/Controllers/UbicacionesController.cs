using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVoto.API.Data;
using SistemaVoto.Models;
using SistemaVoto.API.DTOs;

namespace SistemaVoto.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UbicacionesController : ControllerBase
    {
        private readonly VotacionDbContext _context;

        public UbicacionesController(VotacionDbContext context)
        {
            _context = context;
        }

        // GET: api/Ubicaciones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUbicaciones()
        {
            var ubicaciones = await _context.UbicacionesVotacion
                .Include(u => u.VotantesAsignados)
                .Include(u => u.JefesAsignados)
                .Select(u => new
                {
                    u.UbicacionId,
                    u.Nombre,
                    u.Direccion,
                    u.NumeroMesa,
                    u.CapacidadVotantes,
                    u.Activo,
                    TotalVotantes = u.VotantesAsignados.Count,
                    TotalJefes = u.JefesAsignados.Count,
                    VotantesPresentes = u.VotantesAsignados
                        .Count(v => v.Estado == "PresenteEnRecinto" || v.Estado == "VotoEmitido"),
                    VotosEmitidos = u.VotantesAsignados
                        .Count(v => v.Estado == "VotoEmitido")
                })
                .ToListAsync();

            return Ok(ubicaciones);
        }

        // GET: api/Ubicaciones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUbicacion(int id)
        {
            var ubicacion = await _context.UbicacionesVotacion
                .Include(u => u.VotantesAsignados)
                    .ThenInclude(v => v.Usuario)
                .Include(u => u.JefesAsignados)
                    .ThenInclude(j => j.Usuario)
                .Where(u => u.UbicacionId == id)
                .Select(u => new
                {
                    u.UbicacionId,
                    u.Nombre,
                    u.Direccion,
                    u.NumeroMesa,
                    u.CapacidadVotantes,
                    u.Activo,
                    Votantes = u.VotantesAsignados.Select(v => new
                    {
                        v.VotanteId,
                        NombreCompleto = v.Usuario.Nombre + " " + v.Usuario.Apellido,
                        v.Usuario.Cedula,
                        v.Estado
                    }).ToList(),
                    Jefes = u.JefesAsignados.Select(j => new
                    {
                        j.JefeDeJuntaId,
                        NombreCompleto = j.Usuario.Nombre + " " + j.Usuario.Apellido,
                        j.Usuario.Cedula
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (ubicacion == null)
                return NotFound(new { mensaje = "Ubicación no encontrada" });

            return Ok(ubicacion);
        }

        // POST: api/Ubicaciones
        [HttpPost]
        public async Task<ActionResult<object>> PostUbicacion(UbicacionDto dto)
        {
            var ubicacion = new UbicacionVotacion
            {
                Nombre = dto.Nombre,
                Direccion = dto.Direccion,
                NumeroMesa = dto.NumeroMesa,
                CapacidadVotantes = dto.CapacidadVotantes,
                Activo = true
            };

            _context.UbicacionesVotacion.Add(ubicacion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUbicacion), new { id = ubicacion.UbicacionId }, new
            {
                mensaje = "Ubicación creada exitosamente",
                ubicacionId = ubicacion.UbicacionId,
                nombre = ubicacion.Nombre
            });
        }

        // PUT: api/Ubicaciones/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUbicacion(int id, UbicacionDto dto)
        {
            var ubicacion = await _context.UbicacionesVotacion.FindAsync(id);

            if (ubicacion == null)
                return NotFound(new { mensaje = "Ubicación no encontrada" });

            ubicacion.Nombre = dto.Nombre;
            ubicacion.Direccion = dto.Direccion;
            ubicacion.NumeroMesa = dto.NumeroMesa;
            ubicacion.CapacidadVotantes = dto.CapacidadVotantes;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Ubicación actualizada exitosamente" });
        }

        // POST: api/Ubicaciones/asignar-votante
        [HttpPost("asignar-votante")]
        public async Task<IActionResult> AsignarVotante(AsignarVotanteDto dto)
        {
            var ubicacion = await _context.UbicacionesVotacion.FindAsync(dto.UbicacionId);
            if (ubicacion == null)
                return NotFound(new { mensaje = "Ubicación no encontrada" });

            var votante = await _context.Votantes.FindAsync(dto.VotanteId);
            if (votante == null)
                return NotFound(new { mensaje = "Votante no encontrado" });

            votante.UbicacionAsignadaId = dto.UbicacionId;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Votante asignado exitosamente a la ubicación" });
        }

        // POST: api/Ubicaciones/asignar-jefe
        [HttpPost("asignar-jefe")]
        public async Task<IActionResult> AsignarJefe(AsignarJefeDto dto)
        {
            var ubicacion = await _context.UbicacionesVotacion.FindAsync(dto.UbicacionId);
            if (ubicacion == null)
                return NotFound(new { mensaje = "Ubicación no encontrada" });

            var jefe = await _context.JefesDeJunta.FindAsync(dto.JefeDeJuntaId);
            if (jefe == null)
                return NotFound(new { mensaje = "Jefe de Junta no encontrado" });

            jefe.UbicacionAsignadaId = dto.UbicacionId;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Jefe de Junta asignado exitosamente a la ubicación" });
        }

        // PATCH: api/Ubicaciones/5/activar
        [HttpPatch("{id}/activar")]
        public async Task<IActionResult> Activar(int id)
        {
            var ubicacion = await _context.UbicacionesVotacion.FindAsync(id);
            if (ubicacion == null)
                return NotFound(new { mensaje = "Ubicación no encontrada" });

            ubicacion.Activo = true;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Ubicación activada exitosamente" });
        }

        // PATCH: api/Ubicaciones/5/desactivar
        [HttpPatch("{id}/desactivar")]
        public async Task<IActionResult> Desactivar(int id)
        {
            var ubicacion = await _context.UbicacionesVotacion.FindAsync(id);
            if (ubicacion == null)
                return NotFound(new { mensaje = "Ubicación no encontrada" });

            ubicacion.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Ubicación desactivada exitosamente" });
        }

        // DELETE: api/Ubicaciones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUbicacion(int id)
        {
            var ubicacion = await _context.UbicacionesVotacion
                .Include(u => u.VotantesAsignados)
                .Include(u => u.JefesAsignados)
                .FirstOrDefaultAsync(u => u.UbicacionId == id);

            if (ubicacion == null)
                return NotFound(new { mensaje = "Ubicación no encontrada" });

            if (ubicacion.VotantesAsignados.Any())
                return BadRequest(new { mensaje = "No se puede eliminar una ubicación con votantes asignados" });

            if (ubicacion.JefesAsignados.Any())
                return BadRequest(new { mensaje = "No se puede eliminar una ubicación con jefes asignados" });

            _context.UbicacionesVotacion.Remove(ubicacion);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Ubicación eliminada exitosamente" });
        }
    }

    // DTOs
    public class UbicacionDto
    {
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string NumeroMesa { get; set; }
        public int CapacidadVotantes { get; set; }
    }

    //public class AsignarVotanteDto
    //{
    //    public int UbicacionId { get; set; }
    //    public int VotanteId { get; set; }
    //}

    //public class AsignarJefeDto
    //{
    //    public int UbicacionId { get; set; }
    //    public int JefeDeJuntaId { get; set; }
    //}
}