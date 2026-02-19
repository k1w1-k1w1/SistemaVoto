using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace SistemaVoto.API.Services
{
    public class SMSService : ISMSService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SMSService> _logger;

        public SMSService(IConfiguration configuration, ILogger<SMSService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> EnviarCodigoVotoAsync(string telefono, string nombreCompleto, string codigoVoto)
        {
            var mensaje = $@"Hola {nombreCompleto},

Tu código de votación es: {codigoVoto}

Ingresa a la plataforma online y usa este código para emitir tu voto.

IMPORTANTE: Este código es de un solo uso. No lo compartas.

- Sistema de Votación Electoral";

            return await EnviarSMSAsync(telefono, mensaje);
        }

        public async Task<bool> EnviarSMSAsync(string telefono, string mensaje)
        {
            try
            {
                var accountSid = _configuration["SMS:TwilioAccountSid"];
                var authToken = _configuration["SMS:TwilioAuthToken"];
                var twilioNumber = _configuration["SMS:TwilioPhoneNumber"];

                // Si no hay credenciales, simular envío exitoso en desarrollo
                if (string.IsNullOrEmpty(accountSid) || accountSid == "TU_ACCOUNT_SID")
                {
                    _logger.LogWarning($"SMS SIMULADO - Para: {telefono}");
                    _logger.LogInformation($"Mensaje: {mensaje}");
                    return true; // Simular éxito en desarrollo
                }

                TwilioClient.Init(accountSid, authToken);

                var messageResource = await MessageResource.CreateAsync(
                    body: mensaje,
                    from: new PhoneNumber(twilioNumber),
                    to: new PhoneNumber(telefono)
                );

                _logger.LogInformation($"SMS enviado exitosamente a {telefono}. SID: {messageResource.Sid}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al enviar SMS: {ex.Message}");
                return false;
            }
        }
    }
}