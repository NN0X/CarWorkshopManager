using System.Collections.Generic;
using System.Threading.Tasks;
using CarWorkshopManager.Models.Domain;

namespace CarWorkshopManager.Services.Interfaces
{
    public interface ITaskCommentService
    {
        Task<IEnumerable<TaskComment>> GetCommentsForTaskAsync(int serviceTaskId);
        Task AddCommentAsync(int serviceTaskId, string userId, string content);
    }
}
