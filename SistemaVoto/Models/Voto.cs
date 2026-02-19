using System;
using System.ComponentModel.DataAnnotations;

namespace SistemaVoto.Models
{
    public class Voto
    {
        [Key]
        public int VotoId { get; set; }

        [Required]
        public int EleccionId { get; set; }

        [Required]
        public int VotanteId { get; set; }

        // Tipo de voto: "Individual", "Plancha", "Blanco"
        [Required]
        [StringLength(20)]
        public string TipoVoto { get; set; }

        // Para voto individual o plancha
        public int? CandidatoId { get; set; }

        // Para voto en plancha (toda la lista política)
        public int? ListaPoliticaId { get; set; }

        public DateTime FechaEmision { get; set; }

        public string? VotoEncriptado { get; set; }

        // Navegación
        public virtual Eleccion Eleccion { get; set; }
        public virtual Candidato? Candidato { get; set; }
        public virtual Votante Votante { get; set; }
        public virtual ListaPolitica? ListaPolitica { get; set; }

        public Voto()
        {
            FechaEmision = DateTime.UtcNow;
        }

        public void Encriptar()
        {
            var contenido = TipoVoto switch
            {
                "Individual" => $"VOTO:INDIVIDUAL:{CandidatoId}:{Guid.NewGuid()}",
                "Plancha" => $"VOTO:PLANCHA:{ListaPoliticaId}:{Guid.NewGuid()}",
                "Blanco" => $"VOTO:BLANCO:{Guid.NewGuid()}",
                _ => $"VOTO:DESCONOCIDO:{Guid.NewGuid()}"
            };

            VotoEncriptado = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(contenido)
            );
        }
    }
}