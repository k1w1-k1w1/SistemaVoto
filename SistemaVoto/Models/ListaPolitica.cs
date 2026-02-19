using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVoto.Models
{
    public class ListaPolitica
    {
        [Key]
        public int ListaPoliticaId { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        public string? Siglas { get; set; }

        public string? Logo { get; set; }

        public string? Descripcion { get; set; }

        public bool Activo { get; set; }

        // Navegación
        public virtual ICollection<Candidato> Candidatos { get; set; }

        public ListaPolitica()
        {
            Activo = true;
            Candidatos = new HashSet<Candidato>();
        }
    }
}
