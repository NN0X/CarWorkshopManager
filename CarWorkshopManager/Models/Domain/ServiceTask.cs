using System.ComponentModel.DataAnnotations;
using CarWorkshopManager.Models.Identity;
using Microsoft.EntityFrameworkCore;

namespace CarWorkshopManager.Models.Domain;

public class ServiceTask
{
    public int Id { get; set; }

    [Required]
    public int ServiceOrderId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int WorkRateId { get; set; }

    [Required]
    [Precision(6, 2)]
    [Range(0, 1000.00)]
    public decimal WorkHours { get; set; }

    [Required]
    [Precision(8, 2)]
    [Range(0.01, 100000.00)]
    public decimal HourRateNetSnapshot { get; set; }

    [Required]
    [Precision(4, 2)]
    [Range(0.00, 1.00)]
    public decimal VatRateSnapshot { get; set; } 

    [Precision(18, 2)]
    public decimal TotalNet { get; set; }

    [Precision(18, 2)]
    public decimal TotalVat { get; set; }

    [Required]
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public ServiceOrder ServiceOrder { get; set; } = null!;
    public WorkRate WorkRate { get; set; } = null!;
    public ICollection<UsedPart> UsedParts { get; set; } = new List<UsedPart>();
    public ICollection<ApplicationUser> Mechanics { get; set; } = new List<ApplicationUser>();
}
