using System.ComponentModel.DataAnnotations;

namespace CarWorkshopManager.Models.Domain;

public class OrderStatus
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
}
