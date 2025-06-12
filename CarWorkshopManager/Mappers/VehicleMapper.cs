using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.ViewModels.Vehicle;
using Riok.Mapperly.Abstractions;

namespace CarWorkshopManager.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class VehicleMapper
{
    [MapperIgnoreTarget(nameof(Vehicle.VehicleBrand))]
    [MapProperty(nameof(AddVehicleViewModel.Year), nameof(Vehicle.ProductionYear))]
    public partial Vehicle ToVehicle(AddVehicleViewModel vm);

    [MapperIgnoreTarget(nameof(Vehicle.VehicleBrand))]
    [MapProperty(nameof(VehicleEditViewModel.Year), nameof(Vehicle.ProductionYear))]
    public partial void MapToExisting(VehicleEditViewModel vm, [MappingTarget] Vehicle entity);

    [MapProperty(nameof(Vehicle.VehicleBrand.Name), nameof(VehicleEditViewModel.Brand))]
    [MapProperty(nameof(Vehicle.ProductionYear),     nameof(VehicleEditViewModel.Year))]
    public partial VehicleEditViewModel ToEditVm(Vehicle vehicle);

    [MapProperty(nameof(Vehicle.VehicleBrand.Name), nameof(VehicleListItemViewModel.BrandName))]
    public partial VehicleListItemViewModel ToVehicleListItemViewModel(Vehicle vehicle);

    public partial IQueryable<VehicleListItemViewModel>
        ProjectToVehicleListItems(IQueryable<Vehicle> src);
}
