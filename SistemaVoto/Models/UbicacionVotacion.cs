using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVoto.Models
{
    public class UbicacionVotacion
    {
        [Key]
        public int UbicacionId { get; set; }

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; }

        [Required]
        public string Direccion { get; set; }

        [StringLength(10)]
        public string NumeroMesa { get; set; }

        public int CapacidadVotantes { get; set; }

        public bool Activo { get; set; }

        // Navegación
        public virtual ICollection<Votante> VotantesAsignados { get; set; }
        public virtual ICollection<JefeDeJunta> JefesAsignados { get; set; }

        public UbicacionVotacion()
        {
            Activo = true;
            VotantesAsignados = new HashSet<Votante>();
            JefesAsignados = new HashSet<JefeDeJunta>();
        }

        public int ContarVotantesPresentes()
        {
            return VotantesAsignados.Count(v => v.Estado == "PresenteEnRecinto");
        }
    }
}
