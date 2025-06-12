using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using CarWorkshopManager.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CarWorkshopManager.Services.Implementations
{
    public class SmtpEmailWithAttachmentSender : IEmailWithAttachmentSender
    {
        private readonly IConfiguration _config;

        public SmtpEmailWithAttachmentSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(
            string email,
            string subject,
            string htmlMessage,
            byte[] attachmentBytes,
            string attachmentFilename,
            string attachmentType)
        {
            var smtp = _config.GetSection("ReportSettings:Smtp");
            using var message = new MailMessage
            {
                From = new MailAddress(_config["ReportSettings:SenderEmail"]!, _config["ReportSettings:SenderName"]),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            message.To.Add(email);

            using var ms = new MemoryStream(attachmentBytes);
            message.Attachments.Add(new Attachment(ms, attachmentFilename, attachmentType));

            using var client = new SmtpClient(
                smtp["Host"],
                int.Parse(smtp["Port"]!))
            {
                Credentials = new NetworkCredential(smtp["Username"], smtp["Password"]),
                EnableSsl = bool.Parse(smtp["EnableSsl"]!)
            };

            await client.SendMailAsync(message);
        }
    }
}
