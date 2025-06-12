using System;
using System.Threading.Tasks;
using CarWorkshopManager.Data;
using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.Services.Interfaces;

namespace CarWorkshopManager.Services.Implementations
{
    public class OrderCommentService : IOrderCommentService
    {
        private readonly ApplicationDbContext _db;

        public OrderCommentService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddCommentAsync(int serviceOrderId, string authorId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Comment cannot be empty.", nameof(content));

            var comment = new OrderComment
            {
                ServiceOrderId = serviceOrderId,
                AuthorId = authorId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _db.OrderComments.Add(comment);
            await _db.SaveChangesAsync();
        }
    }
}
