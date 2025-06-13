using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CarWorkshopManager.Services.Implementations
{
    public class EmailSender : IEmailSender
    {
        private readonly string _apiKey;
        private readonly EmailAddress _sender;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(
            IConfiguration configuration,
            ILogger<EmailSender> logger)
        {
            _logger = logger;
            _apiKey = configuration["SendGrid:ApiKey"]
                ?? throw new InvalidOperationException("SendGrid API Key is missing");
            var senderEmail = configuration["SendGrid:SenderEmail"]
                ?? throw new InvalidOperationException("Sender Email is missing");
            var senderName = configuration["SendGrid:SenderName"]
                ?? throw new InvalidOperationException("Sender Name is missing");
            _sender = new EmailAddress(senderEmail, senderName);
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogInformation("Sending email to {Email}, subject: {Subject}", email, subject);
            try
            {
                var client = new SendGridClient(_apiKey);
                var msg = MailHelper.CreateSingleEmail(_sender, new EmailAddress(email), subject, null, htmlMessage);
                await client.SendEmailAsync(msg);
                _logger.LogInformation("Email sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", email);
                throw;
            }
        }
    }
}
