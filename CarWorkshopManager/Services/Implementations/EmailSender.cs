using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid.Helpers.Mail;
using SendGrid;

namespace CarWorkshopManager.Services.Implementations;

public class EmailSender : IEmailSender
{
    private readonly string _apiKey;
    private readonly EmailAddress _sender;

    public EmailSender(IConfiguration configuration)
    {
        _apiKey = configuration["SendGrid:ApiKey"] ?? throw new InvalidOperationException("SendGrid API Key is missing");
        var senderEmail = configuration["SendGrid:SenderEmail"] ?? throw new InvalidOperationException("Sender Email is missing");
        var senderName = configuration["SendGrid:SenderName"] ?? throw new InvalidOperationException("Sender Name is missing");
        _sender = new EmailAddress(senderEmail, senderName);
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var client = new SendGridClient(_apiKey);
        var msg = MailHelper.CreateSingleEmail(_sender, new EmailAddress(email), subject, null, htmlMessage);

        await client.SendEmailAsync(msg);
    }
}
