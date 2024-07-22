using AbsenceNotifier.Core.Configurations;
using AbsenceNotifier.Core.DTO.Results;
using AbsenceNotifier.Core.Interfaces;
using AbsenceNotifier.Core.Settings;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Serilog;

namespace AbsenceNotifier.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpConfiguration _smtpConfiguration;

        public EmailService(IOptions<SmtpConfiguration> options)
        {
            _smtpConfiguration = options.Value;
        }
        public async Task<SendEmailResult> SendEmailAsync(string email, string subject, string message)
        {
            var result = new SendEmailResult() { Success = true };

            using (var client = new SmtpClient())
            {
                try
                {
                    using var emailMessage = PrepareMessage(email, subject, message);
                    await client.ConnectAsync(SmtpSettings.Server, SmtpSettings.Port, _smtpConfiguration.EnableSsl);
                    await client.AuthenticateAsync(SmtpSettings.Username, SmtpSettings.Password);
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }
                catch (SmtpCommandException ex)
                {
                    // Handle specific SMTP command exception
                    result.Success = false;
                    if (ex.Message.Contains("5.7.1 Policy rejection", StringComparison.InvariantCulture))
                    {
                        result.Message = $"Email doesn't valid or doesn't exist or was blocked";
                    }
                    else if (ex.Message.Contains("5.1.3"))
                    {
                        result.Message = $"Email not valid";
                    }
                    Log.Logger.Error(ex, result.Message ?? "smtp sending unknown error");
                }
                catch (SmtpProtocolException ex)
                {
                    // Handle SMTP protocol exception
                    result.Success = false;
                    result.Message = $"SMTP protocol exception: {ex.Message}";
                    Log.Logger.Error(ex, result.Message);
                }
                catch (Exception ex)
                {
                    // Handle other exceptions
                    result.Success = false;
                    result.Message = $"An error occurred while sending the email: {ex.Message}";
                    Log.Logger.Error(ex, result.Message);
                }
            }

            return result;
        }

        private MimeMessage PrepareMessage(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            using (var client = new SmtpClient())
            {
                try
                {
                    emailMessage.From.Add(new MailboxAddress("Absence notify", SmtpSettings.From));
                    emailMessage.To.Add(new MailboxAddress("", email));
                    emailMessage.Subject = subject;
                    emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                    {
                        Text = message
                    };

                }
                catch (ArgumentNullException ex)
                {
                    // Handle other exceptions
                    Log.Logger.Error(ex, ex.Message);
                    throw new ArgumentNullException($"Couldn't compose an email to {email}.");
                }
            }

            return emailMessage;
        }

    }
}
