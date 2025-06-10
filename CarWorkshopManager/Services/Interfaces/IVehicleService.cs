using CarWorkshopManager.ViewModels.Vehicle;

namespace CarWorkshopManager.Services.Interfaces;

public interface IVehicleService
{
    Task AddVehicleAsync(AddVehicleViewModel vm);
    Task<List<VehicleListItemViewModel>> GetCustomerVehiclesAsync(int customerId);
    Task<List<VehicleListItemViewModel>> GetAllVehiclesAsync();
    Task<VehicleEditViewModel> GetEditVehicleAsync(int id);
    Task UpdateVehicleAsync(VehicleEditViewModel vm);
    Task UploadVehiclePhotoAsync(int id, IFormFile file);
}