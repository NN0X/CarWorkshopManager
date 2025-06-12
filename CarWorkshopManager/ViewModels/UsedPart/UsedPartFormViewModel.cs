using System.ComponentModel.DataAnnotations;

namespace CarWorkshopManager.ViewModels.UsedPart;

public class UsedPartFormViewModel
{
    [Required]
    public int ServiceTaskId { get; set; }

    [Required]
    [Display(Name = "Część")]
    public int PartId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    [Display(Name = "Ilość")]
    public int Quantity { get; set; }
}
