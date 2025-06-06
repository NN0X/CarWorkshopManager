using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace CarWorkshopManager.Models.Domain;

public class VatRate
{
    public int Id { get; set; }
    
    [Required]
    [Precision(4, 2)]
    [Range(0.00, 1.00)]
    public decimal Rate { get; set; }
    
    [Required]
    [MaxLength(15)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    public DateOnly ValidFrom { get; set; }
    
    public DateOnly? ValidTo { get; set; }
}