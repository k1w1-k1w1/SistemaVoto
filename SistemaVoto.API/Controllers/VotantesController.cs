using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVoto.API.Data;
using SistemaVoto.API.Services;
using SistemaVoto.Models;
using System.Security.Claims;

namespace SistemaVoto.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Votante")]
    public class VotantesController : ControllerBase
    {
        private readonly VotacionDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IPdfService _pdfService;

        public VotantesController(VotacionDbContext context, IEmailService emailService, IPdfService pdfService)
        {
            _context = context;
            _emailService = emailService;
            _pdfService = pdfService;
        }

        // GET: api/Votantes/mi-ubicacion
        [HttpGet("mi-ubicacion")]
        public async Task<IActionResult> ObtenerMiUbicacion()
        {
            var usuarioId = GetUsuarioId();

            var votante = await _context.Votantes
                .Include(v => v.UbicacionAsignada)
                .FirstOrDefaultAsync(v => v.UsuarioId == usuarioId);

            if (votante == null)
                return NotFound(new { mensaje = "Votante no encontrado." });

            if (votante.UbicacionAsignada == null)
                return Ok(new { asignada = false, mensaje = "Aún no tienes una ubicación asignada." });

            var u = votante.UbicacionAsignada;

            return Ok(new
            {
                asignada = true,
                votanteId = votante.VotanteId,
                estadoVotante = votante.Estado,
                fechaPresenciaRecinto = votante.FechaPresenciaRecinto,
                ubicacion = new
                {
                    ubicacionId = u.UbicacionId,
                    nombre = u.Nombre,
                    direccion = u.Direccion,
                    numeroMesa = u.NumeroMesa,
                    activo = u.Activo
                }
            });
        }


        [HttpPost("verificar-codigo")]
        public async Task<ActionResult<object>> VerificarCodigo(VerificarCodigoSeguroDto dto)
        {
            var usuarioId = GetUsuarioId();

            var votante = await _context.Votantes.FirstOrDefaultAsync(v => v.UsuarioId == usuarioId);
            if (votante == null)
                return NotFound(new { mensaje = "Votante no encontrado", esValido = false });

            var codigoVoto = await _context.CodigosVoto
                .Include(c => c.Eleccion)
                .FirstOrDefaultAsync(c => c.Codigo == dto.Codigo
                    && c.VotanteId == votante.VotanteId
                    && c.EleccionId == dto.EleccionId);

            if (codigoVoto == null)
                return NotFound(new { mensaje = "Código no válido", esValido = false });

            if (codigoVoto.Estado != "Generado")
                return BadRequest(new { mensaje = $"El código ya fue {codigoVoto.Estado}", esValido = false });

            if (!codigoVoto.Eleccion.EstaActiva())
                return BadRequest(new { mensaje = "La elección no está activa", esValido = false });

            return Ok(new
            {
                mensaje = "Código válido",
                esValido = true,
                codigoVotoId = codigoVoto.CodigoVotoId,
                eleccion = new
                {
                    codigoVoto.Eleccion.EleccionId,
                    codigoVoto.Eleccion.Nombre,
                    codigoVoto.Eleccion.Tipo
                }
            });
        }


        // GET: api/Votantes/papeleta/{eleccionId}
        [HttpGet("papeleta/{eleccionId}")]
        public async Task<ActionResult<object>> GetPapeleta(int eleccionId)
        {
            var eleccion = await _context.Elecciones
                .Include(e => e.Candidatos)
                    .ThenInclude(c => c.ListaPolitica)
                .FirstOrDefaultAsync(e => e.EleccionId == eleccionId);

            if (eleccion == null)
                return NotFound(new { mensaje = "Elección no encontrada" });

            if (!eleccion.EstaActiva())
                return BadRequest(new { mensaje = "La elección no está activa" });

            // Agrupar candidatos por lista política
            var candidatosPorLista = eleccion.Candidatos
                .Where(c => c.Activo)
                .GroupBy(c => c.ListaPoliticaId)
                .Select(g => new
                {
                    listaPolitica = g.First().ListaPolitica != null ? new
                    {
                        g.First().ListaPolitica.ListaPoliticaId,
                        g.First().ListaPolitica.Nombre,
                        g.First().ListaPolitica.Siglas,
                        g.First().ListaPolitica.Logo
                    } : null,
                    candidatos = g.Select(c => new
                    {
                        c.CandidatoId,
                        c.Nombre,
                        c.Apellido,
                        NombreCompleto = $"{c.Nombre} {c.Apellido}",
                        c.Foto,
                        c.Propuestas
                    }).ToList()
                }).ToList();

            return Ok(new
            {
                eleccion = new
                {
                    eleccion.EleccionId,
                    eleccion.Nombre,
                    eleccion.Tipo,
                    eleccion.FechaFin
                },
                tiposVotoDisponibles = new[]
                {
                    "Individual",  // Seleccionar un candidato específico
                    "Plancha",     // Votar por toda una lista política
                    "Blanco"       // Voto en blanco
                },
                candidatosPorLista
            });
        }

        [HttpPost("emitir-voto")]
        public async Task<ActionResult<object>> EmitirVoto(EmitirVotoSeguroDto dto)
        {
            var usuarioId = GetUsuarioId();

            var votante = await _context.Votantes
                .Include(v => v.Usuario)
                .FirstOrDefaultAsync(v => v.UsuarioId == usuarioId);

            if (votante == null)
                return NotFound(new { mensaje = "Votante no encontrado" });

            if (votante.Estado == "VotoEmitido")
                return BadRequest(new { mensaje = "Ya emitiste tu voto." });

            var codigoVoto = await _context.CodigosVoto
                .Include(c => c.Eleccion)
                .FirstOrDefaultAsync(c => c.CodigoVotoId == dto.CodigoVotoId
                    && c.VotanteId == votante.VotanteId
                    && c.Estado == "Generado");

            if (codigoVoto == null)
                return BadRequest(new { mensaje = "Código de voto inválido o ya utilizado" });

            if (!codigoVoto.Eleccion.EstaActiva())
                return BadRequest(new { mensaje = "La elección no está activa" });

            var yaVoto = await _context.Votos.AnyAsync(v => v.VotanteId == votante.VotanteId
                && v.EleccionId == codigoVoto.EleccionId);

            if (yaVoto)
                return BadRequest(new { mensaje = "Ya emitiste tu voto en esta elección" });

            // Validaciones tipo voto (igual que tienes)
            switch (dto.TipoVoto)
            {
                case "Individual":
                    if (!dto.CandidatoId.HasValue)
                        return BadRequest(new { mensaje = "Debes seleccionar un candidato para voto individual" });
                    break;

                case "Plancha":
                    if (!dto.ListaPoliticaId.HasValue)
                        return BadRequest(new { mensaje = "Debes seleccionar una lista política para voto en plancha" });
                    break;

                case "Blanco":
                    break;

                default:
                    return BadRequest(new { mensaje = "Tipo de voto inválido. Use: Individual, Plancha o Blanco" });
            }

            // Crear voto
            var voto = new Voto
            {
                EleccionId = codigoVoto.EleccionId,
                VotanteId = votante.VotanteId,
                TipoVoto = dto.TipoVoto,
                CandidatoId = dto.TipoVoto == "Individual" ? dto.CandidatoId : null,
                ListaPoliticaId = dto.TipoVoto == "Plancha" ? dto.ListaPoliticaId : null,
                FechaEmision = DateTime.UtcNow
            };

            voto.Encriptar();
            _context.Votos.Add(voto);

            codigoVoto.Estado = "Utilizado";
            codigoVoto.FechaUso = DateTime.UtcNow;

            votante.Estado = "VotoEmitido";

            await _context.SaveChangesAsync();

            // Certificado
            var certificado = new CertificadoVotacion
            {
                VotanteId = votante.VotanteId,
                EleccionId = codigoVoto.EleccionId,
                FechaEmision = DateTime.UtcNow
            };
            _context.CertificadosVotacion.Add(certificado);
            await _context.SaveChangesAsync();

            // Log (con DatosAdicionales)
            _context.LogsActividad.Add(new LogActividad
            {
                UsuarioId = votante.UsuarioId,
                Accion = "EmisionVoto",
                Descripcion = $"Voto emitido exitosamente. Tipo: {dto.TipoVoto}",
                Fecha = DateTime.UtcNow,
                DatosAdicionales = ""
            });
            await _context.SaveChangesAsync();

            // Enviar PDF (tu bloque actual)
            bool correoCertificadoEnviado = false;
            try
            {
                var certDb = await _context.CertificadosVotacion
                    .Include(c => c.Votante).ThenInclude(v => v.Usuario)
                    .Include(c => c.Eleccion)
                    .FirstOrDefaultAsync(c => c.CertificadoId == certificado.CertificadoId);

                if (certDb != null)
                {
                    var user = certDb.Votante.Usuario;
                    var pdfBytes = _pdfService.GenerarCertificadoPdf(
                        $"{user.Nombre} {user.Apellido}",
                        user.Cedula,
                        certDb.Eleccion.Nombre,
                        certDb.Eleccion.Tipo,
                        certDb.NumeroConfirmacion,
                        certDb.FechaEmision
                    );

                    correoCertificadoEnviado = await _emailService.EnviarCertificadoPdfAsync(
                        user.Email,
                        "Tu certificado de votación (PDF)",
                        $"<p>Hola <b>{user.Nombre}</b>, adjunto está tu certificado PDF.</p><p><b>Confirmación:</b> {certDb.NumeroConfirmacion}</p>",
                        pdfBytes,
                        $"Certificado_{certDb.NumeroConfirmacion}.pdf"
                    );
                }
            }
            catch { /* no tumba el voto */ }

            return Ok(new
            {
                mensaje = "¡Voto emitido exitosamente!",
                correoCertificadoEnviado,
                certificado = new
                {
                    certificado.CertificadoId,
                    certificado.NumeroConfirmacion,
                    certificado.FechaEmision,
                    tipoVoto = dto.TipoVoto
                }
            });
        }


        // GET: api/Votantes/certificado/{votanteId}/{eleccionId}
        [HttpGet("certificado/{votanteId}/{eleccionId}")]
        public async Task<ActionResult<object>> GetCertificado(int votanteId, int eleccionId)
        {
            var certificado = await _context.CertificadosVotacion
                .Include(c => c.Votante)
                    .ThenInclude(v => v.Usuario)
                .Include(c => c.Eleccion)
                .FirstOrDefaultAsync(c => c.VotanteId == votanteId
                    && c.EleccionId == eleccionId);

            if (certificado == null)
                return NotFound(new { mensaje = "Certificado no encontrado" });

            return Ok(new
            {
                mensaje = "Certificado de votación",
                certificado = new
                {
                    certificado.CertificadoId,
                    certificado.NumeroConfirmacion,
                    certificado.FechaEmision,
                    votante = new
                    {
                        nombreCompleto = $"{certificado.Votante.Usuario.Nombre} {certificado.Votante.Usuario.Apellido}",
                        certificado.Votante.Usuario.Cedula
                    },
                    eleccion = new
                    {
                        certificado.Eleccion.Nombre,
                        certificado.Eleccion.Tipo
                    }
                }
            });
        }

        private int GetUsuarioId()
        {
            // 1) Lo estándar: NameIdentifier (lo que YA trae tu token)
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 2) Fallbacks por si en otro token lo mandas diferente
            userIdStr ??= User.FindFirstValue("UsuarioId");
            userIdStr ??= User.FindFirstValue("userId");
            userIdStr ??= User.FindFirstValue("id");

            if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
                throw new UnauthorizedAccessException("Token sin claim de UsuarioId/NameIdentifier");

            return userId;
        }

    }

    // DTOs

    public class VerificarCodigoSeguroDto
    {
        public string Codigo { get; set; }
        public int EleccionId { get; set; }
    }



    public class EmitirVotoSeguroDto
    {
        public int CodigoVotoId { get; set; }
        public string TipoVoto { get; set; } // Individual | Plancha | Blanco
        public int? CandidatoId { get; set; }
        public int? ListaPoliticaId { get; set; }
    }



}