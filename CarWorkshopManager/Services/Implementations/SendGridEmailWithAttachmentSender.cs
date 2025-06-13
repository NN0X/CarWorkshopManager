using CarWorkshopManager.Services.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CarWorkshopManager.Services.Implementations
{
    public class SendGridEmailWithAttachmentSender : IEmailWithAttachmentSender
    {
        private readonly string _apiKey;
        private readonly EmailAddress _sender;
        private readonly ILogger<SendGridEmailWithAttachmentSender> _logger;

        public SendGridEmailWithAttachmentSender(
            IConfiguration configuration,
            ILogger<SendGridEmailWithAttachmentSender> logger)
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

        public async Task SendEmailAsync(
            string email,
            string subject,
            string htmlMessage,
            byte[] attachmentBytes,
            string attachmentFilename,
            string attachmentType)
        {
            _logger.LogInformation(
                "SendEmailAsync called: to={Email}, subject={Subject}, attachment={Filename} ({Bytes:n0} bytes)",
                email, subject, attachmentFilename, attachmentBytes?.LongLength ?? 0);

            try
            {
                var client = new SendGridClient(_apiKey);
                var msg = MailHelper.CreateSingleEmail(
                    _sender,
                    new EmailAddress(email),
                    subject,
                    plainTextContent: null,
                    htmlContent: htmlMessage);

                var base64 = Convert.ToBase64String(attachmentBytes);
                msg.AddAttachment(attachmentFilename, base64, attachmentType);

                var response = await client.SendEmailAsync(msg);
                _logger.LogInformation("SendEmailAsync: response {StatusCode}", response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "SendEmailAsync failed: to={Email}, subject={Subject}, attachment={Filename}",
                    email, subject, attachmentFilename);
                throw;
            }
        }
    }
}
