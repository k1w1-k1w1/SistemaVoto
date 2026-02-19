using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVoto.API.Data;
using SistemaVoto.Models;

namespace SistemaVoto.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListasPoliticasController : ControllerBase
    {
        private readonly VotacionDbContext _context;

        public ListasPoliticasController(VotacionDbContext context)
        {
            _context = context;
        }

        // GET: api/ListasPoliticas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetListasPoliticas()
        {
            var listas = await _context.ListasPoliticas
                .Include(l => l.Candidatos)
                .Select(l => new
                {
                    l.ListaPoliticaId,
                    l.Nombre,
                    l.Siglas,
                    l.Logo,
                    l.Descripcion,
                    l.Activo,
                    TotalCandidatos = l.Candidatos.Count
                })
                .ToListAsync();

            return Ok(listas);
        }

        // GET: api/ListasPoliticas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetListaPolitica(int id)
        {
            var lista = await _context.ListasPoliticas
                .Include(l => l.Candidatos)
                .Where(l => l.ListaPoliticaId == id)
                .Select(l => new
                {
                    l.ListaPoliticaId,
                    l.Nombre,
                    l.Siglas,
                    l.Logo,
                    l.Descripcion,
                    l.Activo,
                    Candidatos = l.Candidatos.Select(c => new
                    {
                        c.CandidatoId,
                        NombreCompleto = c.Nombre + " " + c.Apellido,
                        c.Cedula,
                        c.Foto,
                        c.Propuestas,
                        c.Activo
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (lista == null)
                return NotFound(new { mensaje = "Lista política no encontrada" });

            return Ok(lista);
        }

        // POST: api/ListasPoliticas
        [HttpPost]
        public async Task<ActionResult<object>> PostListaPolitica(ListaPoliticaDto dto)
        {
            // Validar nombre único
            if (await _context.ListasPoliticas.AnyAsync(l => l.Nombre == dto.Nombre))
                return BadRequest(new { mensaje = "Ya existe una lista política con ese nombre" });

            var lista = new ListaPolitica
            {
                Nombre = dto.Nombre,
                Siglas = dto.Siglas,
                Logo = dto.Logo,
                Descripcion = dto.Descripcion,
                Activo = true
            };

            _context.ListasPoliticas.Add(lista);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetListaPolitica), new { id = lista.ListaPoliticaId }, new
            {
                mensaje = "Lista política creada exitosamente",
                listaPoliticaId = lista.ListaPoliticaId,
                nombre = lista.Nombre
            });
        }

        // PUT: api/ListasPoliticas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutListaPolitica(int id, ListaPoliticaDto dto)
        {
            var lista = await _context.ListasPoliticas.FindAsync(id);

            if (lista == null)
                return NotFound(new { mensaje = "Lista política no encontrada" });

            // Validar nombre único (excepto la misma lista)
            if (await _context.ListasPoliticas.AnyAsync(l => l.Nombre == dto.Nombre && l.ListaPoliticaId != id))
                return BadRequest(new { mensaje = "Ya existe una lista política con ese nombre" });

            lista.Nombre = dto.Nombre;
            lista.Siglas = dto.Siglas;
            lista.Logo = dto.Logo;
            lista.Descripcion = dto.Descripcion;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Lista política actualizada exitosamente" });
        }

        // PATCH: api/ListasPoliticas/5/activar
        [HttpPatch("{id}/activar")]
        public async Task<IActionResult> Activar(int id)
        {
            var lista = await _context.ListasPoliticas.FindAsync(id);
            if (lista == null)
                return NotFound(new { mensaje = "Lista política no encontrada" });

            lista.Activo = true;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Lista política activada exitosamente" });
        }

        // PATCH: api/ListasPoliticas/5/desactivar
        [HttpPatch("{id}/desactivar")]
        public async Task<IActionResult> Desactivar(int id)
        {
            var lista = await _context.ListasPoliticas.FindAsync(id);
            if (lista == null)
                return NotFound(new { mensaje = "Lista política no encontrada" });

            lista.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Lista política desactivada exitosamente" });
        }

        // DELETE: api/ListasPoliticas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteListaPolitica(int id)
        {
            var lista = await _context.ListasPoliticas
                .Include(l => l.Candidatos)
                .FirstOrDefaultAsync(l => l.ListaPoliticaId == id);

            if (lista == null)
                return NotFound(new { mensaje = "Lista política no encontrada" });

            if (lista.Candidatos.Any())
                return BadRequest(new { mensaje = "No se puede eliminar una lista que tiene candidatos asignados" });

            _context.ListasPoliticas.Remove(lista);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Lista política eliminada exitosamente" });
        }
    }

    // DTO
    public class ListaPoliticaDto
    {
        public string Nombre { get; set; }
        public string? Siglas { get; set; }
        public string? Logo { get; set; }
        public string? Descripcion { get; set; }
    }
}