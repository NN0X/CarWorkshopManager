using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace CarWorkshopManager.Models.Domain;

public class WorkRate
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Precision(8, 2)]
    [Range(0, 100000)]
    public decimal HourRateNet { get; set; }

    [Required]
    public int VatRateId { get; set; }

    [Required]
    public DateOnly ValidFrom { get; set; }

    public DateOnly? ValidTo { get; set; }

    public VatRate VatRate { get; set; } = null!;
}
