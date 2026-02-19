using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVoto.API.Data;
using SistemaVoto.Models;

namespace SistemaVoto.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EleccionesController : ControllerBase
    {
        private readonly VotacionDbContext _context;

        public EleccionesController(VotacionDbContext context)
        {
            _context = context;
        }

        //SOLO ADMIN: lista completa de elecciones
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetElecciones()
        {
            var elecciones = await _context.Elecciones
                .Include(e => e.Candidatos)
                .Include(e => e.Votos)
                .Select(e => new
                {
                    e.EleccionId,
                    e.Nombre,
                    e.Descripcion,
                    e.FechaInicio,
                    e.FechaFin,
                    e.Tipo,
                    e.Estado,
                    e.ResultadosPublicos,
                    TotalCandidatos = e.Candidatos.Count,
                    TotalVotos = e.Votos.Count
                })
                .ToListAsync();

            return Ok(elecciones);
        }

        // SOLO ADMIN: detalle completo
        [Authorize(Roles = "Administrador")]
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetEleccion(int id)
        {
            var eleccion = await _context.Elecciones
                .Include(e => e.Candidatos)
                    .ThenInclude(c => c.ListaPolitica)
                .Include(e => e.Votos)
                .Where(e => e.EleccionId == id)
                .Select(e => new
                {
                    e.EleccionId,
                    e.Nombre,
                    e.Descripcion,
                    e.FechaInicio,
                    e.FechaFin,
                    e.Tipo,
                    e.Estado,
                    e.ResultadosPublicos,
                    Candidatos = e.Candidatos.Select(c => new
                    {
                        c.CandidatoId,
                        c.Nombre,
                        c.Apellido,
                        c.Cedula,
                        c.Foto,
                        c.Propuestas,
                        c.Activo,
                        ListaPolitica = c.ListaPolitica != null ? new
                        {
                            c.ListaPolitica.ListaPoliticaId,
                            c.ListaPolitica.Nombre,
                            c.ListaPolitica.Siglas,
                            c.ListaPolitica.Logo
                        } : null,
                        TotalVotos = c.Votos.Count
                    }).ToList(),
                    TotalVotos = e.Votos.Count
                })
                .FirstOrDefaultAsync();

            if (eleccion == null)
                return NotFound(new { mensaje = "Elección no encontrada" });

            return Ok(eleccion);
        }

        // ✅ VOTANTE + JEFE + ADMIN: elecciones activas
        [Authorize(Roles = "Administrador,JefeDeJunta,Votante")]
        [HttpGet("activas")]
        public async Task<ActionResult<IEnumerable<object>>> GetEleccionesActivas()
        {
            var ahora = DateTime.UtcNow;

            var elecciones = await _context.Elecciones
                .Where(e => e.Estado == "Activa" && e.FechaInicio <= ahora && e.FechaFin >= ahora)
                .Include(e => e.Candidatos)
                .Select(e => new
                {
                    e.EleccionId,
                    e.Nombre,
                    e.Descripcion,
                    e.FechaInicio,
                    e.FechaFin,
                    e.Tipo,
                    TotalCandidatos = e.Candidatos.Count
                })
                .ToListAsync();

            return Ok(elecciones);
        }

        // ✅ SOLO ADMIN: crear elección
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<ActionResult<Eleccion>> PostEleccion(EleccionDto dto)
        {
            if (dto.FechaInicio >= dto.FechaFin)
                return BadRequest(new { mensaje = "La fecha de inicio debe ser anterior a la fecha de fin" });

            if (dto.FechaInicio < DateTime.UtcNow)
                return BadRequest(new { mensaje = "La fecha de inicio no puede ser en el pasado" });

            var eleccion = new Eleccion
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                Tipo = dto.Tipo,
                Estado = "Configuracion",
                ResultadosPublicos = false
            };

            _context.Elecciones.Add(eleccion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEleccion), new { id = eleccion.EleccionId }, new
            {
                mensaje = "Elección creada exitosamente",
                eleccionId = eleccion.EleccionId,
                nombre = eleccion.Nombre,
                estado = eleccion.Estado
            });
        }

        // ✅ SOLO ADMIN: actualizar elección
        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEleccion(int id, EleccionDto dto)
        {
            var eleccion = await _context.Elecciones.FindAsync(id);
            if (eleccion == null)
                return NotFound(new { mensaje = "Elección no encontrada" });

            if (eleccion.Estado == "Activa" || eleccion.Estado == "Finalizada")
                return BadRequest(new { mensaje = $"No se puede editar una elección en estado {eleccion.Estado}" });

            eleccion.Nombre = dto.Nombre;
            eleccion.Descripcion = dto.Descripcion;
            eleccion.FechaInicio = dto.FechaInicio;
            eleccion.FechaFin = dto.FechaFin;
            eleccion.Tipo = dto.Tipo;

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Elección actualizada exitosamente" });
        }

        // ✅ SOLO ADMIN: iniciar elección
        [Authorize(Roles = "Administrador")]
        [HttpPost("{id}/iniciar")]
        public async Task<IActionResult> IniciarEleccion(int id)
        {
            var eleccion = await _context.Elecciones
                .Include(e => e.Candidatos)
                .FirstOrDefaultAsync(e => e.EleccionId == id);

            if (eleccion == null)
                return NotFound(new { mensaje = "Elección no encontrada" });

            if (eleccion.Estado != "Configuracion")
                return BadRequest(new { mensaje = $"Solo se puede iniciar una elección en estado Configuracion. Estado actual: {eleccion.Estado}" });

            if (eleccion.Candidatos.Count < 2)
                return BadRequest(new { mensaje = "Debe haber al menos 2 candidatos para iniciar la elección" });

            if (eleccion.FechaInicio > DateTime.UtcNow)
                return BadRequest(new { mensaje = "La fecha de inicio aún no ha llegado" });

            eleccion.Estado = "Activa";
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Elección iniciada exitosamente", estado = eleccion.Estado });
        }

        // ✅ SOLO ADMIN: finalizar elección
        [Authorize(Roles = "Administrador")]
        [HttpPost("{id}/finalizar")]
        public async Task<IActionResult> FinalizarEleccion(int id)
        {
            var eleccion = await _context.Elecciones.FindAsync(id);

            if (eleccion == null)
                return NotFound(new { mensaje = "Elección no encontrada" });

            if (eleccion.Estado != "Activa")
                return BadRequest(new { mensaje = $"Solo se puede finalizar una elección en estado Activa. Estado actual: {eleccion.Estado}" });

            eleccion.Estado = "Finalizada";
            eleccion.ResultadosPublicos = true;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Elección finalizada exitosamente", estado = eleccion.Estado });
        }

        // ✅ PÚBLICO: resultados (pero respeta tu regla ResultadosPublicos/Finalizada)
        [AllowAnonymous]
        [HttpGet("{id}/resultados")]
        public async Task<ActionResult<object>> GetResultados(int id)
        {
            var eleccion = await _context.Elecciones
                .Include(e => e.Candidatos)
                    .ThenInclude(c => c.Votos)
                .Include(e => e.Candidatos)
                    .ThenInclude(c => c.ListaPolitica)
                .FirstOrDefaultAsync(e => e.EleccionId == id);

            if (eleccion == null)
                return NotFound(new { mensaje = "Elección no encontrada" });

            if (!eleccion.ResultadosPublicos && eleccion.Estado != "Finalizada")
                return BadRequest(new { mensaje = "Los resultados aún no están disponibles" });

            var totalVotos = eleccion.Candidatos.Sum(c => c.Votos.Count);

            var resultados = eleccion.Candidatos
                .Select(c => new
                {
                    c.CandidatoId,
                    NombreCompleto = $"{c.Nombre} {c.Apellido}",
                    c.Foto,
                    ListaPolitica = c.ListaPolitica?.Nombre,
                    Siglas = c.ListaPolitica?.Siglas,
                    CantidadVotos = c.Votos.Count,
                    Porcentaje = totalVotos > 0 ? Math.Round((c.Votos.Count * 100.0 / totalVotos), 2) : 0
                })
                .OrderByDescending(r => r.CantidadVotos)
                .ToList();

            return Ok(new
            {
                eleccion = new
                {
                    eleccion.EleccionId,
                    eleccion.Nombre,
                    eleccion.Tipo,
                    eleccion.Estado
                },
                totalVotos,
                resultados
            });
        }

        // ✅ SOLO ADMIN: eliminar
        [Authorize(Roles = "Administrador")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEleccion(int id)
        {
            var eleccion = await _context.Elecciones.FindAsync(id);

            if (eleccion == null)
                return NotFound(new { mensaje = "Elección no encontrada" });

            if (eleccion.Estado == "Activa" || eleccion.Estado == "Finalizada")
                return BadRequest(new { mensaje = $"No se puede eliminar una elección en estado {eleccion.Estado}" });

            _context.Elecciones.Remove(eleccion);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Elección eliminada exitosamente" });
        }
    }

    // DTO
    public class EleccionDto
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Tipo { get; set; }
    }
}
