using System.Threading.Tasks;

namespace CarWorkshopManager.Services.Interfaces
{
    public interface IEmailWithAttachmentSender
    {
        Task SendEmailAsync(
            string email,
            string subject,
            string htmlMessage,
            byte[] attachmentBytes,
            string attachmentFilename,
            string attachmentType);
    }
}
