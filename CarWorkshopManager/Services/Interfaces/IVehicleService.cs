using CarWorkshopManager.ViewModels.Vehicle;

namespace CarWorkshopManager.Services.Interfaces;

public interface IVehicleService
{
    Task AddVehicleAsync(AddVehicleViewModel vm);
    Task<List<VehicleListItemViewModel>> GetCustomerVehicles(int customerId);
}