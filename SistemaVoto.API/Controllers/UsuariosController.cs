using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVoto.API.Data;
using SistemaVoto.API.Services;
using SistemaVoto.Models;

namespace SistemaVoto.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly VotacionDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;

        public UsuariosController(VotacionDbContext context, IJwtService jwtService, IEmailService emailService)
        {
            _context = context;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios.ToListAsync();
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
        }

        // POST: api/Usuarios/registro
        [HttpPost("registro")]
        public async Task<ActionResult<Usuario>> Registro(RegistroDto dto)
        {
            // Validar si el email ya existe
            if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { mensaje = "El email ya está registrado" });

            // Validar si la cédula ya existe
            if (await _context.Usuarios.AnyAsync(u => u.Cedula == dto.Cedula))
                return BadRequest(new { mensaje = "La cédula ya está registrada" });

            // Crear el usuario
            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Cedula = dto.Cedula,
                Email = dto.Email,
                Telefono = dto.Telefono,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Rol = "Votante",
                Estado = "PendienteVerificacion",
                FechaCreacion = DateTime.UtcNow,
                TokenVerificacion = Guid.NewGuid().ToString()
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Crear el votante asociado
            var votante = new Votante
            {
                UsuarioId = usuario.UsuarioId,
                Estado = "Registrado"
            };

            _context.Votantes.Add(votante);
            await _context.SaveChangesAsync();

            // Enviar correo de verificación
            var verifyLink = "http://localhost:5268/pages/confirmar.html" +
                 $"?email={Uri.EscapeDataString(usuario.Email)}" +
                 $"&token={Uri.EscapeDataString(usuario.TokenVerificacion)}";


            var ok = await _emailService.EnviarVerificacionCuentaAsync(
                usuario.Email,
                usuario.Nombre,
                verifyLink
            );

            if (!ok)
            {
                // Aquí SIEMPRE retornamos algo -> evita el CS0161
                return StatusCode(500, new
                {
                    mensaje = "Usuario creado, pero no se pudo enviar el correo de verificación."
                });
            }

            // Retorno final OK
            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.UsuarioId }, new
            {
                mensaje = "Usuario registrado. Revisa tu correo para verificar la cuenta.",
                usuarioId = usuario.UsuarioId,
                email = usuario.Email
            });
        }

        // POST: api/Usuarios/login
        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDto dto)
        {
            // Buscar usuario por email
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (usuario == null)
            {
                return Unauthorized(new { mensaje = "Email o contraseña incorrectos" });
            }

            // Verificar contraseña
            if (!usuario.VerificarPassword(dto.Password))
            {
                return Unauthorized(new { mensaje = "Email o contraseña incorrectos" });
            }

            // Verificar estado de la cuenta
            if (usuario.Estado != "Activa")
            {
                return BadRequest(new { mensaje = $"La cuenta está en estado: {usuario.Estado}" });
            }

            // Registrar log de actividad
            var log = new LogActividad
            {
                UsuarioId = usuario.UsuarioId,
                Accion = "Login",
                Descripcion = $"Usuario {usuario.Email} inició sesión",
                Fecha = DateTime.UtcNow,
                DatosAdicionales = ""
            };
            _context.LogsActividad.Add(log);
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerarToken(usuario);

            return Ok(new
            {
                mensaje = "Login exitoso",
                token,
                expiracion = DateTime.UtcNow.AddMinutes(480),
                usuario = new
                {
                    usuarioId = usuario.UsuarioId,
                    nombre = usuario.Nombre,
                    apellido = usuario.Apellido,
                    email = usuario.Email,
                    rol = usuario.Rol
                }
            });
        }

        // PUT: api/Usuarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            if (id != usuario.UsuarioId)
            {
                return BadRequest();
            }

            _context.Entry(usuario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Usuarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.UsuarioId == id);
        }

        [HttpGet("confirmar")]
        public async Task<IActionResult> ConfirmarCuenta([FromQuery] string email, [FromQuery] string token)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado." });

            if (usuario.Estado == "Activa")
                return Ok(new { mensaje = "La cuenta ya está activa." });

            if (usuario.TokenVerificacion != token)
                return BadRequest(new { mensaje = "Token inválido." });

            usuario.Estado = "Activa";
            usuario.TokenVerificacion = "";
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "✅ Cuenta activada correctamente. Ya puedes iniciar sesión." });
        }
    }

    // DTOs
    public class RegistroDto
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Cedula { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Password { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}