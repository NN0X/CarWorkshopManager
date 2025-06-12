using CarWorkshopManager.Data;
using CarWorkshopManager.Mappers;
using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.Vehicle;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CarWorkshopManager.Services.Implementations
{
    public class VehicleService : IVehicleService
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly VehicleMapper _mapper;
        private readonly ILogger<VehicleService> _logger;

        public VehicleService(
            ApplicationDbContext db,
            IWebHostEnvironment env,
            VehicleMapper mapper,
            ILogger<VehicleService> logger)
        {
            _db = db;
            _env = env;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task AddVehicleAsync(AddVehicleViewModel vm)
        {
            _logger.LogInformation("AddVehicleAsync called for CustomerId={CustomerId}", vm.CustomerId);
            var vehicle = _mapper.ToVehicle(vm);
            var brandName = vm.Brand.ToLowerInvariant();
            var existing = await _db.VehicleBrands.SingleOrDefaultAsync(b => b.Name == brandName);
            vehicle.VehicleBrand = existing ?? new VehicleBrand { Name = brandName };

            _db.Vehicles.Add(vehicle);
            await _db.SaveChangesAsync();
            _logger.LogInformation("AddVehicleAsync: created vehicle Id={Id}", vehicle.Id);
        }

        public async Task<List<VehicleListItemViewModel>> GetCustomerVehiclesAsync(int customerId)
        {
            _logger.LogInformation("GetCustomerVehiclesAsync called for CustomerId={CustomerId}", customerId);
            return await _mapper
                .ProjectToVehicleListItems(_db.Vehicles.Where(v => v.CustomerId == customerId))
                .ToListAsync();
        }

        public async Task<List<VehicleListItemViewModel>> GetAllVehiclesAsync()
        {
            _logger.LogInformation("GetAllVehiclesAsync called");
            return await _mapper
                .ProjectToVehicleListItems(_db.Vehicles)
                .ToListAsync();
        }

        public async Task<VehicleEditViewModel?> GetEditVehicleAsync(int id)
        {
            _logger.LogInformation("GetEditVehicleAsync called for Id={Id}", id);
            var vehicle = await _db.Vehicles.Include(v => v.VehicleBrand)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle is null)
            {
                _logger.LogWarning("GetEditVehicleAsync: not found Id={Id}", id);
                return null;
            }
            return _mapper.ToEditVm(vehicle);
        }

        public async Task UpdateVehicleAsync(VehicleEditViewModel vm)
        {
            _logger.LogInformation("UpdateVehicleAsync called for Id={Id}", vm.Id);
            var vehicle = await _db.Vehicles.Include(v => v.VehicleBrand)
                .FirstOrDefaultAsync(v => v.Id == vm.Id);
            if (vehicle is null)
            {
                _logger.LogWarning("UpdateVehicleAsync: not found Id={Id}", vm.Id);
                return;
            }

            var brandName = vm.Brand.ToLowerInvariant();
            var brand = await _db.VehicleBrands.SingleOrDefaultAsync(b => b.Name == brandName)
                        ?? new VehicleBrand { Name = brandName };
            vehicle.VehicleBrand = brand;
            _mapper.MapToExisting(vm, vehicle);
            await _db.SaveChangesAsync();
            _logger.LogInformation("UpdateVehicleAsync: updated Id={Id}", vm.Id);
        }

        public async Task UploadVehiclePhotoAsync(int id, IFormFile file)
        {
            _logger.LogInformation("UploadVehiclePhotoAsync called for Id={Id}", id);
            var v = await _db.Vehicles.FindAsync(id);
            if (v is null || file is null || file.Length == 0)
            {
                _logger.LogWarning("UploadVehiclePhotoAsync: invalid vehicle or file");
                return;
            }

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
            {
                _logger.LogWarning("UploadVehiclePhotoAsync: unsupported extension {Ext}", ext);
                return;
            }

            var fileName = $"{Guid.NewGuid()}{ext}";
            var savePath = Path.Combine(_env.WebRootPath, "uploads", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
            await using var stream = new FileStream(savePath, FileMode.Create);
            await file.CopyToAsync(stream);

            v.ImageUrl = "/uploads/" + fileName;
            await _db.SaveChangesAsync();
            _logger.LogInformation("UploadVehiclePhotoAsync: saved photo {FileName} for {Id}", fileName, id);
        }
    }
}
