using CarWorkshopManager.ViewModels.Vehicle;

namespace CarWorkshopManager.ViewModels.Customer;

public class CustomerDetailsViewModel
{
    public int Id { get; set; }
    public string FirstName   { get; set; } = string.Empty;
    public string LastName    { get; set; } = string.Empty;
    public string Email       { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public List<VehicleListItemViewModel> Vehicles { get; set; } = new();
}