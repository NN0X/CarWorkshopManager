using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.ViewModels.ServiceOrder;
using Riok.Mapperly.Abstractions;

namespace CarWorkshopManager.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ServiceOrderMapper
{
    public partial ServiceOrder ToServiceOrder(CreateServiceOrderViewModel model);

    [MapProperty(nameof(ServiceOrder.RegistrationNumberSnapshot), nameof(ServiceOrderListItemViewModel.RegistrationNumber))]
    [MapProperty(nameof(ServiceOrder.CustomerNameSnapshot), nameof(ServiceOrderListItemViewModel.CustomerName))]
    [MapProperty(nameof(ServiceOrder.Status.Name), nameof(ServiceOrderListItemViewModel.StatusName))]
    public partial ServiceOrderListItemViewModel ToServiceOrderListItemViewModel(ServiceOrder model);

    [MapProperty(nameof(ServiceOrder.CustomerNameSnapshot), nameof(ServiceOrderDetailsViewModel.CustomerName))]
    [MapProperty(nameof(ServiceOrder.RegistrationNumberSnapshot), nameof(ServiceOrderDetailsViewModel.RegistrationNumber))]
    [MapProperty(nameof(ServiceOrder.Status.Name), nameof(ServiceOrderDetailsViewModel.StatusName))]
    [MapProperty(nameof(ServiceOrder.Tasks), nameof(ServiceOrderDetailsViewModel.Tasks))]
    public partial ServiceOrderDetailsViewModel ToServiceOrderDetailsViewModel(ServiceOrder order);
}
