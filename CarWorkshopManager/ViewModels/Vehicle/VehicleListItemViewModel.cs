namespace CarWorkshopManager.ViewModels.Vehicle;

public class VehicleListItemViewModel
{
    public int Id { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Vin { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public int ProductionYear { get; set; }
    public int Mileage { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public string? ImageUrl { get; set; }
}