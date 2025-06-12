using CarWorkshopManager.Data;
using CarWorkshopManager.Mappers;
using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.Vehicle;
using Microsoft.EntityFrameworkCore;

namespace CarWorkshopManager.Services.Implementations;

public class VehicleService : IVehicleService
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly VehicleMapper _mapper;

    public VehicleService(ApplicationDbContext db, IWebHostEnvironment env, VehicleMapper mapper)
    {
        _db = db;
        _mapper = mapper;
        _env = env;
    }

     public async Task AddVehicleAsync(AddVehicleViewModel vm)
        {
            var vehicle = _mapper.ToVehicle(vm);
            
            var brandName = vm.Brand.ToLowerInvariant();
            var existing  = await _db.VehicleBrands
                                     .SingleOrDefaultAsync(b => b.Name == brandName);

            vehicle.VehicleBrand = existing ?? new VehicleBrand { Name = brandName };

            _db.Vehicles.Add(vehicle);
            await _db.SaveChangesAsync();
        }
     
        public async Task<List<VehicleListItemViewModel>> GetCustomerVehiclesAsync(int customerId)
        {
            return await _mapper
                   .ProjectToVehicleListItems(_db.Vehicles.Where(v => v.CustomerId == customerId))
                   .ToListAsync();
        }
    
        public async Task<List<VehicleListItemViewModel>> GetAllVehiclesAsync()
        {
            return await _mapper
                   .ProjectToVehicleListItems(_db.Vehicles)
                   .ToListAsync();
        }
        
        public async Task<VehicleEditViewModel?> GetEditVehicleAsync(int id)
        {
            var vehicle = await _db.Vehicles
                                   .Include(v => v.VehicleBrand)
                                   .FirstOrDefaultAsync(v => v.Id == id);

            return vehicle is null ? null : _mapper.ToEditVm(vehicle);
        }

        public async Task UpdateVehicleAsync(VehicleEditViewModel vm)
        {
            var vehicle = await _db.Vehicles
                                   .Include(v => v.VehicleBrand)
                                   .FirstOrDefaultAsync(v => v.Id == vm.Id);
            
            if (vehicle is null) 
                return;
            
            var brandName = vm.Brand.ToLowerInvariant();
            var brand = await _db.VehicleBrands.SingleOrDefaultAsync(b => b.Name == brandName)
                       ?? new VehicleBrand { Name = brandName };

            vehicle.VehicleBrand = brand;
    
            _mapper.MapToExisting(vm, vehicle);

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
