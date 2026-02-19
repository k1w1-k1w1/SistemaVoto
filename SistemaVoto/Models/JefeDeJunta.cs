using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVoto.Models;

namespace SistemaVoto.Models
{
    public class JefeDeJunta
    {
        [Key]
        public int JefeDeJuntaId { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        public int? UbicacionAsignadaId { get; set; }

        public DateTime FechaAsignacion { get; set; }

        // Navegación
        public virtual Usuario Usuario { get; set; }
        public virtual UbicacionVotacion UbicacionAsignada { get; set; }
        public virtual ICollection<CodigoVoto> CodigosGenerados { get; set; }
        public virtual ICollection<Incidencia> IncidenciasReportadas { get; set; }

        public JefeDeJunta()
        {
            FechaAsignacion = DateTime.Now;
            CodigosGenerados = new HashSet<CodigoVoto>();
            IncidenciasReportadas = new HashSet<Incidencia>();
        }

        public CodigoVoto GenerarCodigoParaVotante(Votante votante, Eleccion eleccion)
        {
            var codigo = new CodigoVoto
            {
                VotanteId = votante.VotanteId,
                EleccionId = eleccion.EleccionId,
                JefeDeJuntaId = JefeDeJuntaId,
                Codigo = GenerarCodigoUnico(),
                FechaGeneracion = DateTime.Now,
                Estado = "Generado"
            };

            CodigosGenerados.Add(codigo);
            votante.MarcarComoPresente();

            return codigo;
        }

        private string GenerarCodigoUnico()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
