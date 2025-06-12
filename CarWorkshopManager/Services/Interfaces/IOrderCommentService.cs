using System.Threading.Tasks;

namespace CarWorkshopManager.Services.Interfaces
{
    public interface IOrderCommentService
    {
        Task AddCommentAsync(int serviceOrderId, string authorId, string content);
    }
}
