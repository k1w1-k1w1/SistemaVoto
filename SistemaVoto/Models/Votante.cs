using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVoto.Models
{
    public class Votante
    {
        [Key]
        public int VotanteId { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        public int? UbicacionAsignadaId { get; set; }

        [StringLength(50)]
        public string Estado { get; set; } // "Registrado", "PresenteEnRecinto", "CodigoGenerado", "VotoEmitido"

        public DateTime? FechaPresenciaRecinto { get; set; }

        // Navegación
        public virtual Usuario Usuario { get; set; }
        public virtual UbicacionVotacion UbicacionAsignada { get; set; }
        public virtual ICollection<CodigoVoto> CodigosVoto { get; set; }
        public virtual ICollection<Voto> Votos { get; set; }
        public virtual ICollection<CertificadoVotacion> Certificados { get; set; }

        public Votante()
        {
            Estado = "Registrado";
            CodigosVoto = new HashSet<CodigoVoto>();
            Votos = new HashSet<Voto>();
            Certificados = new HashSet<CertificadoVotacion>();
        }

        public bool YaVoto(int eleccionId)
        {
            return Votos.Any(v => v.EleccionId == eleccionId);
        }

        public void MarcarComoPresente()
        {
            Estado = "PresenteEnRecinto";
            FechaPresenciaRecinto = DateTime.Now;
        }
    }
}
