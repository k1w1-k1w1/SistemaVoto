using SistemaVoto.Models;

namespace SistemaVoto.API.Services
{
    public interface IJwtService
    {
        string GenerarToken(Usuario usuario);
        int? ObtenerUsuarioId(string token);
        string? ObtenerRol(string token);
    }
}