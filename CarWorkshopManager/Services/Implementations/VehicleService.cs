using CarWorkshopManager.Data;
using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.Vehicle;
using Microsoft.EntityFrameworkCore;

namespace CarWorkshopManager.Services.Implementations;

public class VehicleService : IVehicleService
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public VehicleService(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task AddVehicleAsync(AddVehicleViewModel vm)
    {
        var brandNormalized = vm.Brand.ToLower();
        var brand = await _db.VehicleBrands.SingleOrDefaultAsync(b => b.Name == brandNormalized);
        if (brand == null)
            brand = (await _db.VehicleBrands.AddAsync(new VehicleBrand { Name = brandNormalized })).Entity;

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

    public async Task<List<VehicleListItemViewModel>> GetCustomerVehiclesAsync(int customerId)
    {
        return await _db.Vehicles
            .Where(v => v.CustomerId == customerId)
            .Select(v => new VehicleListItemViewModel
            {
                Id = v.Id,
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

    public async Task<List<VehicleListItemViewModel>> GetAllVehiclesAsync()
    {
        return await _db.Vehicles
            .Select(v => new VehicleListItemViewModel
            {
                Id = v.Id,
                BrandName = v.VehicleBrand.Name,
                Model = v.Model,
                Vin = v.Vin,
                RegistrationNumber = v.RegistrationNumber,
                ProductionYear = v.ProductionYear,
                Mileage = v.Mileage,
                CreatedAt = v.CreatedAt,
                ImageUrl = v.ImageUrl
            })
            .ToListAsync();
    }

    public async Task<VehicleEditViewModel?> GetEditVehicleAsync(int id)
    {
        var v = await _db.Vehicles.Include(v => v.VehicleBrand)
                                  .FirstOrDefaultAsync(v => v.Id == id);

        if (v is null)
            return null;

        return new VehicleEditViewModel {
                  Id = v.Id,
                  Brand = v.VehicleBrand.Name,
                  Model = v.Model,
                  Vin = v.Vin,
                  RegistrationNumber = v.RegistrationNumber,
                  Year = v.ProductionYear,
                  Mileage = v.Mileage 
        };
    }

    public async Task UpdateVehicleAsync(VehicleEditViewModel vm)
    {
        var vehicle = await _db.Vehicles
            .Include(v => v.VehicleBrand)
            .FirstOrDefaultAsync(v => v.Id == vm.Id);
        if (vehicle == null) 
            return;

        var brandNormalized = vm.Brand.ToLower();
        var brand = await _db.VehicleBrands.SingleOrDefaultAsync(b => b.Name == brandNormalized);
        if (brand == null)
            brand = (await _db.VehicleBrands.AddAsync(new VehicleBrand { Name = brandNormalized })).Entity;


        vehicle.VehicleBrand = brand;
        vehicle.Model = vm.Model;
        vehicle.Vin = vm.Vin;
        vehicle.RegistrationNumber = vm.RegistrationNumber;
        vehicle.ProductionYear = vm.Year;
        vehicle.Mileage = vm.Mileage;

        await _db.SaveChangesAsync();
    }

    public async Task UploadVehiclePhotoAsync(int id, IFormFile file)
    {
        var v = await _db.Vehicles.FindAsync(id);
        if (v == null || file == null || file.Length == 0) 
            return;

        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext is not ".png" and not ".jpg" and not ".jpeg") 
            return;

        var fileName = $"{Guid.NewGuid()}{ext}";
        var savePath = Path.Combine(_env.WebRootPath, "uploads", fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

        await using var stream = new FileStream(savePath, FileMode.Create);
        await file.CopyToAsync(stream);

        v.ImageUrl = "/uploads/" + fileName;
        await _db.SaveChangesAsync();
    }
}
