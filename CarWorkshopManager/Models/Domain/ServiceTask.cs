using System.ComponentModel.DataAnnotations;
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
    
    // *** Snapshots in case of the rates & VAT change in the future [not a redundancy!] *** //
    [Required]
    [Precision(8, 2)]
    [Range(0.01, 100000.00)]
    public decimal HourRateNet { get; set; }
    
    [Required]
    public int VatRateId { get; set; } 
    
    // *** Frozen totals (to avoid rounding issues) *** //
    [Precision(18, 2)]
    public decimal TotalNet { get; set; }
    
    [Precision(18, 2)]
    public decimal TotalVat { get; set; }
    
    [Required]
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? CompletedAt { get; set; }


    public ServiceOrder ServiceOrder { get; set; } = null!;
    public WorkRate WorkRate { get; set; } = null!;
    public VatRate VatRate { get; set; } = null!;
    public ICollection<UsedPart> UsedParts { get; set; } = new List<UsedPart>();
}