using SendGrid;
using SendGrid.Helpers.Mail;
using System;


namespace SistemaVoto.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> EnviarCodigoVotoAsync(string email, string nombreCompleto, string codigoVoto)
        {
            var subject = "Tu código de votación - Sistema Electoral";
            var htmlContent = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #2E75B6;'>Sistema de Votación Electrónica</h2>
                        <p>Hola <strong>{nombreCompleto}</strong>,</p>
                        <p>Tu identidad ha sido verificada exitosamente en el recinto electoral.</p>
                        <p>Tu código único de votación es:</p>
                        <div style='background-color: #f0f0f0; padding: 20px; text-align: center; font-size: 32px; font-weight: bold; letter-spacing: 5px; margin: 20px 0;'>
                            {codigoVoto}
                        </div>
                        <p><strong>Instrucciones:</strong></p>
                        <ol>
                            <li>Ingresa a la plataforma de votación online</li>
                            <li>Introduce este código cuando se te solicite</li>
                            <li>Selecciona tu candidato preferido</li>
                            <li>Confirma tu voto</li>
                        </ol>
                        <p style='color: #d9534f;'><strong>Importante:</strong></p>
                        <ul>
                            <li>Este código es de <strong>un solo uso</strong></li>
                            <li>No compartas este código con nadie</li>
                            <li>El código expira después de ser usado</li>
                        </ul>
                        <p style='margin-top: 30px; font-size: 12px; color: #999;'>
                            Este es un mensaje automático del Sistema de Votación Electrónica.
                        </p>
                    </div>
                </body>
                </html>
            ";

            return await EnviarEmailAsync(email, subject, htmlContent);
        }

        public async Task<bool> EnviarEmailAsync(string toEmail, string subject, string htmlContent)
        {
            try
            {
                var apiKey = _configuration["Email:SendGridApiKey"];
                var fromEmail = _configuration["Email:FromEmail"];
                var fromName = _configuration["Email:FromName"];

                // Si no hay API Key, simular envío exitoso en desarrollo
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("No se configuró SendGridApiKey.");
                    return false;
                }

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(fromEmail, fromName);
                var to = new EmailAddress(toEmail);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlContent);

                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode == System.Net.HttpStatusCode.Accepted ||
                    response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogInformation($"Email enviado exitosamente a {toEmail}");
                    return true;
                }
                else
                {
                    _logger.LogError($"Error al enviar email. StatusCode: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Excepción al enviar email: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EnviarVerificacionCuentaAsync(string email, string nombre, string link)
        {
            var subject = "Verifica tu cuenta - Sistema de Votación";

            var htmlContent = $@"
        <html>
        <body style='font-family: Arial, sans-serif;'>
            <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                <h2 style='color: #2E75B6;'>Sistema de Votación Electrónica</h2>
                <p>Hola <strong>{nombre}</strong>,</p>
                <p>Gracias por registrarte.</p>
                <p>Para activar tu cuenta, haz clic en el siguiente botón:</p>
                
                <div style='text-align:center; margin: 20px 0;'>
                    <a href='{link}' 
                       style='background-color:#2E75B6; color:white; padding:12px 20px; text-decoration:none; border-radius:5px;'>
                        Confirmar Cuenta
                    </a>
                </div>

                <p>Si no solicitaste este registro, puedes ignorar este mensaje.</p>

                <p style='margin-top: 30px; font-size: 12px; color: #999;'>
                    Este es un mensaje automático del Sistema de Votación Electrónica.
                </p>
            </div>
        </body>
        </html>
    ";

            return await EnviarEmailAsync(email, subject, htmlContent);
        }

        public async Task<bool> EnviarCertificadoPdfAsync(
    string toEmail,
    string subject,
    string htmlContent,
    byte[] pdfBytes,
    string fileName = "CertificadoVotacion.pdf")
        {
            try
            {
                var apiKey = _configuration["Email:SendGridApiKey"];
                var fromEmail = _configuration["Email:FromEmail"];
                var fromName = _configuration["Email:FromName"];

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogError("SendGridApiKey no configurada.");
                    return false;
                }

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(fromEmail, fromName);
                var to = new EmailAddress(toEmail);

                var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlContent);

                // ✅ Adjuntar PDF (base64)
                var base64 = Convert.ToBase64String(pdfBytes);
                msg.AddAttachment(fileName, base64, "application/pdf");

                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode == System.Net.HttpStatusCode.Accepted ||
                    response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogInformation($"Certificado enviado a {toEmail}");
                    return true;
                }

                _logger.LogError($"Error SendGrid: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción enviando certificado PDF");
                return false;
            }
        }



    }
}