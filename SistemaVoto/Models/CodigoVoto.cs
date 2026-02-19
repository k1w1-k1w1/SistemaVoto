using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVoto.Models;

namespace SistemaVoto.Models
{
    public class CodigoVoto
    {
        [Key]
        public int CodigoVotoId { get; set; }

        [Required]
        public int VotanteId { get; set; }

        [Required]
        public int EleccionId { get; set; }

        public int? JefeDeJuntaId { get; set; }

        [Required]
        [StringLength(8)]
        public string Codigo { get; set; }

        public DateTime FechaGeneracion { get; set; }

        public DateTime? FechaUso { get; set; }

        [StringLength(50)]
        public string Estado { get; set; } // "Generado", "Utilizado", "Expirado", "Invalidado"

        // Navegación
        public virtual Votante Votante { get; set; }
        public virtual Eleccion Eleccion { get; set; }
        public virtual JefeDeJunta JefeDeJunta { get; set; }

        public CodigoVoto()
        {
            FechaGeneracion = DateTime.Now;
            Estado = "Generado";
        }

        public bool EsValido()
        {
            return Estado == "Generado" && !FechaUso.HasValue;
        }

        public void Utilizar()
        {
            if (!EsValido())
                throw new InvalidOperationException("El código ya fue utilizado o está inválido.");

            Estado = "Utilizado";
            FechaUso = DateTime.Now;
        }
    }
}
