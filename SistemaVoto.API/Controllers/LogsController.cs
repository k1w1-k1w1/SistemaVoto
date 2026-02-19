using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVoto.API.Data;

namespace SistemaVoto.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly VotacionDbContext _context;

        public LogsController(VotacionDbContext context)
        {
            _context = context;
        }

        // GET: api/Logs
        [HttpGet]
        public async Task<ActionResult<object>> GetLogs(
            [FromQuery] int? usuarioId,
            [FromQuery] string? accion,
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 50)
        {
            var query = _context.LogsActividad
                .Include(l => l.Usuario)
                .AsQueryable();

            if (usuarioId.HasValue)
                query = query.Where(l => l.UsuarioId == usuarioId);

            if (!string.IsNullOrEmpty(accion))
                query = query.Where(l => l.Accion == accion);

            if (desde.HasValue)
                query = query.Where(l => l.Fecha >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(l => l.Fecha <= hasta.Value);

            var total = await query.CountAsync();

            var logs = await query
                .OrderByDescending(l => l.Fecha)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Select(l => new
                {
                    l.LogId,
                    l.Accion,
                    l.Descripcion,
                    l.DireccionIP,
                    l.Fecha,
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
                tamano,
                totalPaginas = (int)Math.Ceiling(total / (double)tamano),
                logs
            });
        }

        // GET: api/Logs/estadisticas
        [HttpGet("estadisticas")]
        public async Task<ActionResult<object>> GetEstadisticas()
        {
            var accionesPorTipo = await _context.LogsActividad
                .GroupBy(l => l.Accion)
                .Select(g => new
                {
                    Accion = g.Key,
                    Total = g.Count()
                })
                .OrderByDescending(g => g.Total)
                .ToListAsync();

            var logsHoy = await _context.LogsActividad
                .CountAsync(l => l.Fecha.Date == DateTime.UtcNow.Date);

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
                    Usuario = l.Usuario != null
                        ? l.Usuario.Nombre + " " + l.Usuario.Apellido
                        : "Sistema"
                })
                .ToListAsync();

            return Ok(new
            {
                totalLogs = await _context.LogsActividad.CountAsync(),
                logsHoy,
                accionesPorTipo,
                ultimosLogs
            });
        }
    }
}