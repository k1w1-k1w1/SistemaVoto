namespace SistemaVoto.API.Services
{
    public interface IPdfService
    {
        byte[] GenerarCertificadoPdf(
            string nombreCompleto,
            string cedula,
            string eleccionNombre,
            string eleccionTipo,
            string numeroConfirmacion,
            DateTime fechaEmisionUtc
        );
    }
}
