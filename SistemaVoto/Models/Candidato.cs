using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVoto.Models;

namespace SistemaVoto.Models
{
    public class Candidato
    {
        [Key]
        public int CandidatoId { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(100)]
        public string Apellido { get; set; }

        [Required]
        [StringLength(20)]
        public string Cedula { get; set; }

        public string? Foto { get; set; }

        public int? ListaPoliticaId { get; set; }

        public string Propuestas { get; set; }

        [Required]
        public int EleccionId { get; set; }

        public bool Activo { get; set; }

        // Navegación
        public virtual Eleccion Eleccion { get; set; }
        public virtual ListaPolitica ListaPolitica { get; set; }
        public virtual ICollection<Voto> Votos { get; set; }

        public Candidato()
        {
            Activo = true;
            Votos = new HashSet<Voto>();
        }

        public string NombreCompleto => $"{Nombre} {Apellido}";

        public int ContarVotos()
        {
            return Votos.Count;
        }
    }
}
