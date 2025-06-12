using CarWorkshopManager.Data;
using CarWorkshopManager.Models.Domain;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarWorkshopManager.Services.Interfaces;

public class TaskCommentService : ITaskCommentService
{
    private readonly ApplicationDbContext _db;
    public TaskCommentService(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<TaskComment>> GetCommentsForTaskAsync(int serviceTaskId)
    {
        return await _db.TaskComments
                        .Where(c => c.ServiceTaskId == serviceTaskId)
                        .Include(c => c.Author)
                        .OrderBy(c => c.CreatedAt)
                        .ToListAsync();
    }

    public async Task AddCommentAsync(int serviceTaskId, string userId, string content)
    {
        var comment = new TaskComment
        {
            ServiceTaskId = serviceTaskId,
            AuthorId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };
        _db.TaskComments.Add(comment);
        await _db.SaveChangesAsync();
    }
}
