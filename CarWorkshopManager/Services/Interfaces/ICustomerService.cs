using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.ViewModels.Customer;

namespace CarWorkshopManager.Services.Interfaces;

public interface ICustomerService
{
    Task AddCustomerAsync(CreateCustomerViewModel model);
    Task<List<CustomerListItemViewModel>> GetAllCustomersAsync();
    Task<CustomerListItemViewModel> GetCustomerAsync(int customerId);
    Task<CustomerDetailsViewModel?> GetCustomerDetailsAsync(int customerId);

}