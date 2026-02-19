using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVoto.Models
{
    public class Usuario
    {
        [Key]
        public int UsuarioId { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(100)]
        public string Apellido { get; set; }

        [Required]
        [StringLength(20)]
        public string Cedula { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }  

        [Required]
        public string PasswordHash { get; set; }

        public string? FotoPerfil { get; set; }

        [Required]
        [StringLength(50)]
        public string Rol { get; set; } // "Votante", "JefeDeJunta", "Administrador"

        [StringLength(50)]
        public string Estado { get; set; } // "PendienteVerificacion", "Activa", "Bloqueada", "Suspendida"

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaVerificacion { get; set; }

        public string? TokenVerificacion { get; set; }

        public DateTime? TokenExpiracion { get; set; }

        // Navegación
        public virtual Votante Votante { get; set; }
        public virtual JefeDeJunta JefeDeJunta { get; set; }
        public virtual ICollection<LogActividad> LogsActividad { get; set; }

        public Usuario()
        {
            FechaCreacion = DateTime.Now;
            Estado = "PendienteVerificacion";
            LogsActividad = new HashSet<LogActividad>();
        }

        public bool VerificarPassword(string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
        }

        public void GenerarTokenRecuperacion()
        {
            TokenVerificacion = Guid.NewGuid().ToString();
            TokenExpiracion = DateTime.Now.AddMinutes(30);
        }

        public bool TokenEsValido()
        {
            return !string.IsNullOrEmpty(TokenVerificacion)
                && TokenExpiracion.HasValue
                && TokenExpiracion.Value > DateTime.Now;
        }
    }
}
