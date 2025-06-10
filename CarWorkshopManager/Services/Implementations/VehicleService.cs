using CarWorkshopManager.Data;
using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.Vehicle;
using Microsoft.EntityFrameworkCore;

namespace CarWorkshopManager.Services.Implementations;

public class VehicleService : IVehicleService
{
    private readonly ApplicationDbContext _db;

    public VehicleService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddVehicleAsync(AddVehicleViewModel vm)
    {
        var brand = await _db.VehicleBrands.SingleOrDefaultAsync(b => b.Name == vm.Brand.ToLower());
        if (brand == null)
            brand = (await _db.VehicleBrands.AddAsync(new VehicleBrand { Name = vm.Brand.ToLower() })).Entity;

        var vehicle = new Vehicle
        {
            CustomerId = vm.CustomerId,
            VehicleBrand = brand,
            Model = vm.Model,
            Vin = vm.Vin,
            RegistrationNumber = vm.RegistrationNumber,
            ProductionYear = vm.Year,
            Mileage = vm.Mileage
        };
        
        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync();
    }

    public async Task<List<VehicleListItemViewModel>> GetCustomerVehicles(int customerId)
    {
        return await _db.Vehicles
            .Where(v => v.CustomerId == customerId)
            .Select(v => new VehicleListItemViewModel
            {
                BrandName = v.VehicleBrand.Name,
                Model = v.Model,
                Vin = v.Vin,
                RegistrationNumber = v.RegistrationNumber,
                ProductionYear = v.ProductionYear,
                Mileage = v.Mileage,
                CreatedAt = v.CreatedAt
            })
            .ToListAsync();
    }
}