using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVoto.Models;

namespace SistemaVoto.Models
{
    public class Eleccion
    {
        [Key]
        public int EleccionId { get; set; }

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [StringLength(50)]
        public string Tipo { get; set; } // "Presidencial", "Legislativa", "Municipal", "Referendo", "Consulta"

        [StringLength(50)]
        public string Estado { get; set; } // "Configuracion", "Activa", "Finalizada", "Cancelada"

        public bool ResultadosPublicos { get; set; }

        // Navegación
        public virtual ICollection<Candidato> Candidatos { get; set; }
        public virtual ICollection<Voto> Votos { get; set; }
        public virtual ICollection<CodigoVoto> CodigosVoto { get; set; }

        public Eleccion()
        {
            Estado = "Configuracion";
            ResultadosPublicos = false;
            Candidatos = new HashSet<Candidato>();
            Votos = new HashSet<Voto>();
            CodigosVoto = new HashSet<CodigoVoto>();
        }

        public bool EstaActiva()
        {
            return Estado == "Activa"
                && DateTime.Now >= FechaInicio
                && DateTime.Now <= FechaFin;
        }

        public void Iniciar()
        {
            if (Estado != "Configuracion")
                throw new InvalidOperationException("Solo se puede iniciar una elección en configuración.");

            if (Candidatos.Count < 2)
                throw new InvalidOperationException("Debe haber al menos 2 candidatos.");

            Estado = "Activa";
        }

        public void Finalizar()
        {
            if (Estado != "Activa")
                throw new InvalidOperationException("Solo se puede finalizar una elección activa.");

            Estado = "Finalizada";
        }

        public Dictionary<int, int> ObtenerResultados()
        {
            if (!ResultadosPublicos && Estado != "Finalizada")
                throw new InvalidOperationException("Los resultados no están disponibles aún.");

            return Votos
                .Where(v => v.CandidatoId.HasValue)
                .GroupBy(v => v.CandidatoId!.Value)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}
