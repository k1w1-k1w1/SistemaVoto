using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVoto.Models;

namespace SistemaVoto.Models
{
    public class Incidencia
    {
        [Key]
        public int IncidenciaId { get; set; }

        [Required]
        public int JefeDeJuntaId { get; set; }

        public int? VotanteId { get; set; }

        [Required]
        [StringLength(50)]
        public string Tipo { get; set; } // "VotanteNoEncontrado", "SuplantacionIdentidad", "DocumentoInvalido", etc.

        [Required]
        public string Descripcion { get; set; }

        public DateTime FechaReporte { get; set; }

        [StringLength(50)]
        public string Estado { get; set; } // "Pendiente", "EnRevision", "Resuelta", "Rechazada"

        public string Resolucion { get; set; }

        // Navegación
        public virtual JefeDeJunta JefeDeJunta { get; set; }
        public virtual Votante Votante { get; set; }

        public Incidencia()
        {
            FechaReporte = DateTime.Now;
            Estado = "Pendiente";
        }

        public void Resolver(string resolucion)
        {
            Estado = "Resuelta";
            Resolucion = resolucion;
        }
    }
}
