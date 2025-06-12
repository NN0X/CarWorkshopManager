using CarWorkshopManager.ViewModels.ServiceTasks;
using CarWorkshopManager.ViewModels.UsedPart;

namespace CarWorkshopManager.Services.Interfaces;

public interface IServiceTaskService
{
    Task<int> AddServiceTaskAsync(ServiceTaskFormViewModel model);
    Task<int> AddUsedPartAsync(UsedPartFormViewModel model, string currentUserId);
    Task<int> GetTaskServiceOrderServiceIdAsync(int serviceTaskId);
    Task RecalculateTotalsAsync(int serviceOrderId);
}
