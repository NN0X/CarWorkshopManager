using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace CarWorkshopManager.Models.Domain;

public class UsedPart
{
    public int Id { get; set; }

    [Required]
    public int ServiceTaskId { get; set; }

    [Required]
    public int PartId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    [Required]
    [Precision(10, 2)]
    [Range(0.01, 10000000)]
    public decimal UnitPriceNetSnapshot { get; set; }

    [Required]
    [Precision(4, 2)]
    [Range(0.00, 1.00)]
    public decimal VatRateSnapshot { get; set; }

    [Precision(18, 2)]
    public decimal TotalNet { get; set; }

    [Precision(18, 2)]
    public decimal TotalVat { get; set; }

    public ServiceTask ServiceTask { get; set; } = null!;
    public Part Part { get; set; } = null!;
}
