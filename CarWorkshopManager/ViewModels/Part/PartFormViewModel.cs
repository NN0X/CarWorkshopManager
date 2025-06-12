using System.ComponentModel.DataAnnotations;

namespace CarWorkshopManager.ViewModels.Part;

public class PartFormViewModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 10000000)]
    public decimal UnitPriceNet { get; set; }

    [Required]
    public int VatRateId { get; set; }
}
