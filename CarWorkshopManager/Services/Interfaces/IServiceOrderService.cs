using System.Security.Claims;
using System.Threading.Tasks;
using CarWorkshopManager.ViewModels.ServiceOrder;

namespace CarWorkshopManager.Services.Interfaces
{
    public interface IServiceOrderService
    {
        Task<List<ServiceOrderListItemViewModel>> GetAllServiceOrdersAsync();
        Task<int> CreateOrderAsync(CreateServiceOrderViewModel createVm, string userId);
        Task<ServiceOrderDetailsViewModel?> GetOrderDetailsAsync(int id);
        Task<bool> ChangeStatusAsync(int id, string newStatus, string userId);
        Task PopulateDetailsViewModelAsync(ServiceOrderDetailsViewModel detailsVm, ClaimsPrincipal currentUser);
        Task<RepairCostReportViewModel> GetRepairCostReportAsync(DateTime? month, int? vehicleId);
        Task<MonthlyRepairSummaryReportViewModel> GetMonthlyRepairSummaryAsync(DateTime month);
        Task<List<ServiceOrderListItemViewModel>> GetOpenServiceOrdersAsync();
    }
}
