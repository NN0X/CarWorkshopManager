using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.ViewModels.Vehicle;
using Riok.Mapperly.Abstractions;

namespace CarWorkshopManager.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class VehicleMapper
{
    /* ---------- Add VM ➜ Entity ---------- */
    [MapperIgnoreTarget(nameof(Vehicle.VehicleBrand))]                 // marka ustawiana w serwisie
    [MapProperty(nameof(AddVehicleViewModel.Year), nameof(Vehicle.ProductionYear))]
    public partial Vehicle ToVehicle(AddVehicleViewModel vm);

    /* ---------- Edit VM ➜ Entity (update) ---------- */
    [MapperIgnoreTarget(nameof(Vehicle.VehicleBrand))]
    [MapProperty(nameof(VehicleEditViewModel.Year), nameof(Vehicle.ProductionYear))]
    public partial void MapToExisting(VehicleEditViewModel vm, [MappingTarget] Vehicle entity);

    /* ---------- Entity ➜ Edit VM ---------- */
    [MapProperty(nameof(Vehicle.VehicleBrand.Name), nameof(VehicleEditViewModel.Brand))]
    [MapProperty(nameof(Vehicle.ProductionYear),     nameof(VehicleEditViewModel.Year))]
    public partial VehicleEditViewModel ToEditVm(Vehicle vehicle);

    /* ---------- Entity ➜ ListItem VM ---------- */
    [MapProperty(nameof(Vehicle.VehicleBrand.Name), nameof(VehicleListItemViewModel.BrandName))]
    public partial VehicleListItemViewModel ToVehicleListItemViewModel(Vehicle vehicle);

    /* ---------- Projekcja SQL ---------- */
    public partial IQueryable<VehicleListItemViewModel>
        ProjectToVehicleListItems(IQueryable<Vehicle> src);
}