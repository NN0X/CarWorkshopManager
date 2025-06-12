using System.ComponentModel.DataAnnotations;
using CarWorkshopManager.Models.Identity;

namespace CarWorkshopManager.Models.Domain
{
    public class OrderComment
    {
        public int Id { get; set; }

        [Required]
        public int ServiceOrderId { get; set; }
        public ServiceOrder ServiceOrder { get; set; }

        [Required]
        public string AuthorId { get; set; } = string.Empty;
        public ApplicationUser Author { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
