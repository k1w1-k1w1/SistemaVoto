namespace SistemaVoto.API.DTOs
{
    // ====== USUARIOS ======
    public class CambiarRolDto
    {
        public string NuevoRol { get; set; }
    }

    public class CambiarEstadoDto
    {
        public string NuevoEstado { get; set; }
    }

    // ====== INCIDENCIAS ======
    public class ResolverIncidenciaDto
    {
        public string Estado { get; set; }      // "Resuelta" o "Rechazada"
        public string Resolucion { get; set; }  // texto de resolución
    }

    // ====== ASIGNACIONES (por Cédula) ======
    public class AsignarVotanteDto
    {
        public int UbicacionId { get; set; }
        public int VotanteId { get; set; }
    }

    public class AsignarJefeDto
    {
        public int UbicacionId { get; set; }
        public int JefeDeJuntaId { get; set; }
    }

    public class QuitarVotanteDto
    {
        public string Cedula { get; set; }
    }

    public class QuitarJefeDto
    {
        public string Cedula { get; set; }
    }

    public class AsignarVotantePorCedulaDto
    {
        public string Cedula { get; set; }
        public int UbicacionId { get; set; }
    }

    public class QuitarVotantePorCedulaDto
    {
        public string Cedula { get; set; }
    }

    public class AsignarJefePorCedulaDto
    {
        public string Cedula { get; set; }
        public int UbicacionId { get; set; }
    }

    public class QuitarJefePorCedulaDto
    {
        public string Cedula { get; set; }
    }
}
