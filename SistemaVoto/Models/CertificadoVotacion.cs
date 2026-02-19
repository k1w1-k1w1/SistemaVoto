using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVoto.Models;

namespace SistemaVoto.Models
{
    public class CertificadoVotacion
    {
        [Key]
        public int CertificadoId { get; set; }

        [Required]
        public int VotanteId { get; set; }

        [Required]
        public int EleccionId { get; set; }

        [Required]
        public string NumeroConfirmacion { get; set; }

        public DateTime FechaEmision { get; set; }

        // Navegación
        public virtual Votante Votante { get; set; }
        public virtual Eleccion Eleccion { get; set; }

        public CertificadoVotacion()
        {
            FechaEmision = DateTime.Now;
            NumeroConfirmacion = Guid.NewGuid().ToString("N").ToUpper().Substring(0, 16);
        }
    }
}
