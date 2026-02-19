using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVoto.Models
{
    public class LogActividad
    {
        [Key]
        public int LogId { get; set; }

        public int? UsuarioId { get; set; }

        [Required]
        [StringLength(50)]
        public string Accion { get; set; } // "Login", "EmisionVoto", "ConsultaResultados", etc.

        [Required]
        public string Descripcion { get; set; }

        public string? DireccionIP { get; set; }

        public DateTime Fecha { get; set; }

        public string DatosAdicionales { get; set; }

        // Navegación
        public virtual Usuario Usuario { get; set; }

        public LogActividad()
        {
            Fecha = DateTime.Now;
        }

        public static LogActividad Registrar(int? usuarioId, string accion, string descripcion, string ip = null)
        {
            return new LogActividad
            {
                UsuarioId = usuarioId,
                Accion = accion,
                Descripcion = descripcion,
                DireccionIP = ip
            };
        }
    }
}
