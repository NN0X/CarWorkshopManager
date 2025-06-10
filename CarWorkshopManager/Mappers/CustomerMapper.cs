using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.ViewModels.Customer;
using Riok.Mapperly.Abstractions;

namespace CarWorkshopManager.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class CustomerMapper
{
    public partial Customer ToCustomer(CreateCustomerViewModel model);
    public partial CustomerListItemViewModel ToCreateCustomerListItemViewModel(Customer customer);
}