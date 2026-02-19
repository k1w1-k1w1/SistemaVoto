namespace SistemaVoto.API.Services
{
    public interface IEmailService
    {
        Task<bool> EnviarCodigoVotoAsync(string email, string nombreCompleto, string codigoVoto);
        Task<bool> EnviarEmailAsync(string toEmail, string subject, string htmlContent);
        Task<bool> EnviarVerificacionCuentaAsync(string email, string nombre, string link);
        Task<bool> EnviarCertificadoPdfAsync(string toEmail, string subject, string htmlContent, byte[] pdfBytes, string fileName = "CertificadoVotacion.pdf");


    }
}