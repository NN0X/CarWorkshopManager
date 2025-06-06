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
    
    // *** Snapshots in case of the price & VAT change in the future [not a redundancy!] *** //
    [Required]
    [Precision(10, 2)]
    [Range(0, 10000000)]
    public decimal UnitPriceNet { get; set; }
    
    [Required]
    public int VatRateId { get; set; }

    // *** Frozen totals (to avoid rounding issues) *** //
    [Precision(18, 2)]
    public decimal TotalNet { get; set; }
    
    [Precision(18, 2)]
    public decimal TotalVat { get; set; }

    
    public ServiceTask ServiceTask { get; set; } = null!;
    public Part Part { get; set; } = null!;
    public VatRate VatRate { get; set; } = null!;
}