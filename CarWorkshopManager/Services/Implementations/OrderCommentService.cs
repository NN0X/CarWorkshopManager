using CarWorkshopManager.Data;
using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.Services.Interfaces;

namespace CarWorkshopManager.Services.Implementations
{
    public class OrderCommentService : IOrderCommentService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<OrderCommentService> _logger;

        public OrderCommentService(
            ApplicationDbContext db,
            ILogger<OrderCommentService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task AddCommentAsync(int serviceOrderId, string authorId, string content)
        {
            _logger.LogInformation("AddCommentAsync called: OrderId={OrderId}, Author={AuthorId}", serviceOrderId, authorId);
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("AddCommentAsync: empty content");
                throw new ArgumentException("Comment cannot be empty", nameof(content));
            }

            var comment = new OrderComment
            {
                ServiceOrderId = serviceOrderId,
                AuthorId = authorId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _db.OrderComments.Add(comment);
            await _db.SaveChangesAsync();
            _logger.LogInformation("AddCommentAsync: comment saved with Id={Id}", comment.Id);
        }
    }
}
