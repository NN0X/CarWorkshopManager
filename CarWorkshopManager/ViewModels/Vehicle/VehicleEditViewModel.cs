using System.ComponentModel.DataAnnotations;

namespace CarWorkshopManager.ViewModels.Vehicle;

public class VehicleEditViewModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Display(Name = "Marka")]
    public string Brand { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Model { get; set; } = string.Empty;

    [Required]
    [MaxLength(17)]
    public string Vin { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    [Display(Name = "Rejestracja")]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Range(1900, 2100)]
    [Display(Name = "Rok produkcji")]
    public int Year { get; set; }

    [Range(0, 5000000)]
    [Display(Name = "Przebieg [km]")]
    public int Mileage { get; set; }
}
