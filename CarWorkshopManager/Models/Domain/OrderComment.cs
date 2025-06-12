using System.ComponentModel.DataAnnotations;
using CarWorkshopManager.Models.Identity;

namespace CarWorkshopManager.Models.Domain;

public class OrderComment
{
    public int Id { get; set; }

    [Required]
    public int ServiceOrderId { get; set; }

    [Required]
    public string AuthorId { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ServiceOrder ServiceOrder { get; set; } = null!;
    public ApplicationUser Author { get; set; } = null!;
}
