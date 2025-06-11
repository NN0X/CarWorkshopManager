using CarWorkshopManager.ViewModels.ServiceOrder;

namespace CarWorkshopManager.Services.Interfaces;

public interface IServiceOrderService
{
    Task<int> CreateOrderAsync(CreateServiceOrderViewModel createServiceOrderViewModel, string userId);
}