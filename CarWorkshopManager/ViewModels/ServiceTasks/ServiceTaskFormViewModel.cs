using System.ComponentModel.DataAnnotations;

namespace CarWorkshopManager.ViewModels.ServiceTasks;

public class ServiceTaskFormViewModel
{
    [Required]
    public int ServiceOrderId { get; set; }

    [Required, MaxLength(200)]
    [Display(Name = "Opis czynności")]
    public string Description { get; set; } = string.Empty;

    [Required, Range(0.1, 1000)]
    [Display(Name = "Czas pracy [h]")]
    public decimal WorkHours { get; set; }

    [Required]
    [Display(Name = "Stawka robocizny")]
    public int WorkRateId { get; set; }

    [Required(ErrorMessage = "Wybierz przynajmniej jednego mechanika")]
    [Display(Name = "Mechanicy")]
    public List<string> MechanicsIds { get; set; } = new();
}
