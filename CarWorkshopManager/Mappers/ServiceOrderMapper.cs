using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.ViewModels.ServiceOrder;
using Riok.Mapperly.Abstractions;

namespace CarWorkshopManager.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ServiceOrderMapper
{
    public partial ServiceOrder ToServiceOrder(CreateServiceOrderViewModel model);
}