using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVoto.API.Data;
using SistemaVoto.Models;

namespace SistemaVoto.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]  

    public class CandidatosController : ControllerBase
    {
        private readonly VotacionDbContext _context;

        public CandidatosController(VotacionDbContext context)
        {
            _context = context;
        }

        // GET: api/Candidatos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCandidatos()
        {
            var candidatos = await _context.Candidatos
                .Include(c => c.Eleccion)
                .Include(c => c.ListaPolitica)
                .Include(c => c.Votos)
                .Select(c => new
                {
                    c.CandidatoId,
                    c.Nombre,
                    c.Apellido,
                    NombreCompleto = c.Nombre + " " + c.Apellido,
                    c.Cedula,
                    c.Foto,
                    c.Propuestas,
                    c.Activo,
                    Eleccion = new
                    {
                        c.Eleccion.EleccionId,
                        c.Eleccion.Nombre,
                        c.Eleccion.Estado
                    },
                    ListaPolitica = c.ListaPolitica != null ? new
                    {
                        c.ListaPolitica.ListaPoliticaId,
                        c.ListaPolitica.Nombre,
                        c.ListaPolitica.Siglas
                    } : null,
                    TotalVotos = c.Votos.Count
                })
                .ToListAsync();

            return Ok(candidatos);
        }

        // GET: api/Candidatos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetCandidato(int id)
        {
            var candidato = await _context.Candidatos
                .Include(c => c.Eleccion)
                .Include(c => c.ListaPolitica)
                .Include(c => c.Votos)
                .Where(c => c.CandidatoId == id)
                .Select(c => new
                {
                    c.CandidatoId,
                    c.Nombre,
                    c.Apellido,
                    NombreCompleto = c.Nombre + " " + c.Apellido,
                    c.Cedula,
                    c.Foto,
                    c.Propuestas,
                    c.Activo,
                    Eleccion = new
                    {
                        c.Eleccion.EleccionId,
                        c.Eleccion.Nombre,
                        c.Eleccion.Tipo,
                        c.Eleccion.Estado
                    },
                    ListaPolitica = c.ListaPolitica != null ? new
                    {
                        c.ListaPolitica.ListaPoliticaId,
                        c.ListaPolitica.Nombre,
                        c.ListaPolitica.Siglas,
                        c.ListaPolitica.Logo
                    } : null,
                    TotalVotos = c.Votos.Count
                })
                .FirstOrDefaultAsync();

            if (candidato == null)
            {
                return NotFound(new { mensaje = "Candidato no encontrado" });
            }

            return Ok(candidato);
        }

        // GET: api/Candidatos/eleccion/5
        [HttpGet("eleccion/{eleccionId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetCandidatosPorEleccion(int eleccionId)
        {
            var eleccion = await _context.Elecciones.FindAsync(eleccionId);
            if (eleccion == null)
            {
                return NotFound(new { mensaje = "Elección no encontrada" });
            }

            var candidatos = await _context.Candidatos
                .Where(c => c.EleccionId == eleccionId && c.Activo)
                .Include(c => c.ListaPolitica)
                .Include(c => c.Votos)
                .Select(c => new
                {
                    c.CandidatoId,
                    c.Nombre,
                    c.Apellido,
                    NombreCompleto = c.Nombre + " " + c.Apellido,
                    c.Cedula,
                    c.Foto,
                    c.Propuestas,
                    ListaPolitica = c.ListaPolitica != null ? new
                    {
                        c.ListaPolitica.Nombre,
                        c.ListaPolitica.Siglas,
                        c.ListaPolitica.Logo
                    } : null,
                    TotalVotos = c.Votos.Count
                })
                .ToListAsync();

            return Ok(candidatos);
        }

        // POST: api/Candidatos
        [HttpPost]
        public async Task<ActionResult<Candidato>> PostCandidato(CandidatoDto dto)
        {
            // Validar que la elección existe
            var eleccion = await _context.Elecciones.FindAsync(dto.EleccionId);
            if (eleccion == null)
            {
                return NotFound(new { mensaje = "Elección no encontrada" });
            }

            // No permitir agregar candidatos a elecciones activas o finalizadas
            if (eleccion.Estado != "Configuracion")
            {
                return BadRequest(new { mensaje = $"No se pueden agregar candidatos a una elección en estado {eleccion.Estado}" });
            }

            // Validar que la cédula no esté duplicada en la misma elección
            var existeCedula = await _context.Candidatos
                .AnyAsync(c => c.Cedula == dto.Cedula && c.EleccionId == dto.EleccionId);

            if (existeCedula)
            {
                return BadRequest(new { mensaje = "Ya existe un candidato con esa cédula en esta elección" });
            }

            // Validar lista política si se proporciona
            if (dto.ListaPoliticaId.HasValue)
            {
                var listaExiste = await _context.ListasPoliticas.AnyAsync(l => l.ListaPoliticaId == dto.ListaPoliticaId);
                if (!listaExiste)
                {
                    return NotFound(new { mensaje = "Lista política no encontrada" });
                }
            }

            var candidato = new Candidato
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Cedula = dto.Cedula,
                Foto = dto.Foto,
                Propuestas = dto.Propuestas,
                EleccionId = dto.EleccionId,
                ListaPoliticaId = dto.ListaPoliticaId,
                Activo = true
            };

            _context.Candidatos.Add(candidato);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCandidato), new { id = candidato.CandidatoId }, new
            {
                mensaje = "Candidato agregado exitosamente",
                candidatoId = candidato.CandidatoId,
                nombreCompleto = candidato.NombreCompleto
            });
        }

        // PUT: api/Candidatos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCandidato(int id, CandidatoDto dto)
        {
            var candidato = await _context.Candidatos
                .Include(c => c.Eleccion)
                .FirstOrDefaultAsync(c => c.CandidatoId == id);

            if (candidato == null)
            {
                return NotFound(new { mensaje = "Candidato no encontrado" });
            }

            // No permitir editar candidatos en elecciones activas o finalizadas
            if (candidato.Eleccion.Estado != "Configuracion")
            {
                return BadRequest(new { mensaje = $"No se pueden editar candidatos de una elección en estado {candidato.Eleccion.Estado}" });
            }

            candidato.Nombre = dto.Nombre;
            candidato.Apellido = dto.Apellido;
            candidato.Cedula = dto.Cedula;
            candidato.Foto = dto.Foto;
            candidato.Propuestas = dto.Propuestas;
            candidato.ListaPoliticaId = dto.ListaPoliticaId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CandidatoExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return Ok(new { mensaje = "Candidato actualizado exitosamente" });
        }

        // DELETE: api/Candidatos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCandidato(int id)
        {
            var candidato = await _context.Candidatos
                .Include(c => c.Eleccion)
                .Include(c => c.Votos)
                .FirstOrDefaultAsync(c => c.CandidatoId == id);

            if (candidato == null)
            {
                return NotFound(new { mensaje = "Candidato no encontrado" });
            }

            // No permitir eliminar candidatos en elecciones activas o finalizadas
            if (candidato.Eleccion.Estado != "Configuracion")
            {
                return BadRequest(new { mensaje = $"No se pueden eliminar candidatos de una elección en estado {candidato.Eleccion.Estado}" });
            }

            // No permitir eliminar si ya tiene votos
            if (candidato.Votos.Any())
            {
                return BadRequest(new { mensaje = "No se puede eliminar un candidato que ya tiene votos registrados" });
            }

            _context.Candidatos.Remove(candidato);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Candidato eliminado exitosamente" });
        }

        // PATCH: api/Candidatos/5/activar
        [HttpPatch("{id}/activar")]
        public async Task<IActionResult> ActivarCandidato(int id)
        {
            var candidato = await _context.Candidatos.FindAsync(id);
            if (candidato == null)
            {
                return NotFound(new { mensaje = "Candidato no encontrado" });
            }

            candidato.Activo = true;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Candidato activado exitosamente" });
        }

        // PATCH: api/Candidatos/5/desactivar
        [HttpPatch("{id}/desactivar")]
        public async Task<IActionResult> DesactivarCandidato(int id)
        {
            var candidato = await _context.Candidatos.FindAsync(id);
            if (candidato == null)
            {
                return NotFound(new { mensaje = "Candidato no encontrado" });
            }

            candidato.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Candidato desactivado exitosamente" });
        }

        private bool CandidatoExists(int id)
        {
            return _context.Candidatos.Any(e => e.CandidatoId == id);
        }
    }

    // DTO
    public class CandidatoDto
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Cedula { get; set; }
        public string? Foto { get; set; }
        public string? Propuestas { get; set; }
        public int EleccionId { get; set; }
        public int? ListaPoliticaId { get; set; }
    }
}