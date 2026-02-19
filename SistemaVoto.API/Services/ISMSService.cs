namespace SistemaVoto.API.Services
{
    public interface ISMSService
    {
        Task<bool> EnviarCodigoVotoAsync(string telefono, string nombreCompleto, string codigoVoto);
        Task<bool> EnviarSMSAsync(string telefono, string mensaje);
    }
}