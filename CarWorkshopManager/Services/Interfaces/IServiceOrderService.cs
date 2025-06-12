using CarWorkshopManager.ViewModels.ServiceOrder;

namespace CarWorkshopManager.Services.Interfaces;

public interface IServiceOrderService
{
    Task<List<ServiceOrderListItemViewModel>> GetAllServiceOrdersAsync();
    Task<int> CreateOrderAsync(CreateServiceOrderViewModel createServiceOrderViewModel, string userId);
    Task<ServiceOrderDetailsViewModel?> GetOrderDetailsAsync(int id);
    Task<bool> ChangeStatusAsync(int id, string newStatus, string userId);
}