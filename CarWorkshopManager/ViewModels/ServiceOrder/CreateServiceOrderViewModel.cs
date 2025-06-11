using System.ComponentModel.DataAnnotations;
using CarWorkshopManager.ViewModels.Vehicle;

namespace CarWorkshopManager.ViewModels.ServiceOrder;

public class CreateServiceOrderViewModel
{
    [Required]
    public int VehicleId { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = String.Empty;

    public List<VehicleListItemViewModel> Vehicles { get; set; } = new();
}