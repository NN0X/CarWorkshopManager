using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarWorkshopManager.Services.Implementations;
using CarWorkshopManager.Data;
using CarWorkshopManager.ViewModels.Vehicle;
using CarWorkshopManager.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System;

public class VehicleServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment;
    private readonly VehicleService _vehicleService;

    public VehicleServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "VehicleServiceTestDb_" + Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);

        _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
        var tempWebRootPath = Path.Combine(Path.GetTempPath(), "CarWorkshopManagerTests", Guid.NewGuid().ToString());
        _mockWebHostEnvironment.Setup(env => env.WebRootPath).Returns(tempWebRootPath);

        _vehicleService = new VehicleService(_dbContext, _mockWebHostEnvironment.Object);
    }

    [Fact]
    public async Task AddVehicleAsync_AddsNewBrandAndVehicle()
    {
        var addVehicleViewModel = new AddVehicleViewModel
        {
            CustomerId = 1,
            Brand = "Toyota",
            Model = "Camry",
            Vin = "VIN123",
            RegistrationNumber = "REG123",
            Year = 2020,
            Mileage = 50000
        };

        await _vehicleService.AddVehicleAsync(addVehicleViewModel);

        var vehicle = await _dbContext.Vehicles.Include(v => v.VehicleBrand).FirstOrDefaultAsync();
        Xunit.Assert.NotNull(vehicle);
        Xunit.Assert.Equal("toyota", vehicle.VehicleBrand.Name);
        Xunit.Assert.Equal("Camry", vehicle.Model);
        Xunit.Assert.Equal("VIN123", vehicle.Vin);

        var brand = await _dbContext.VehicleBrands.FirstOrDefaultAsync(b => b.Name == "toyota");
        Xunit.Assert.NotNull(brand);
    }

    [Fact]
    public async Task AddVehicleAsync_UsesExistingBrand()
    {
        await _dbContext.VehicleBrands.AddAsync(new VehicleBrand { Name = "honda" });
        await _dbContext.SaveChangesAsync();

        var addVehicleViewModel = new AddVehicleViewModel
        {
            CustomerId = 1,
            Brand = "Honda",
            Model = "Civic",
            Vin = "VIN456",
            RegistrationNumber = "REG456",
            Year = 2021,
            Mileage = 25000
        };

        await _vehicleService.AddVehicleAsync(addVehicleViewModel);

        var vehicle = await _dbContext.Vehicles.Include(v => v.VehicleBrand).FirstOrDefaultAsync();
        Xunit.Assert.NotNull(vehicle);
        Xunit.Assert.Equal("honda", vehicle.VehicleBrand.Name);

        var brandsCount = await _dbContext.VehicleBrands.CountAsync();
        Xunit.Assert.Equal(1, brandsCount); // Should not add a new brand
    }

    [Fact]
    public async Task GetCustomerVehiclesAsync_ReturnsCorrectVehicles()
    {
        var customerId = 1;
        var brand1 = new VehicleBrand { Name = "bmw" };
        var brand2 = new VehicleBrand { Name = "mercedes" };
        await _dbContext.VehicleBrands.AddRangeAsync(brand1, brand2);
        await _dbContext.Vehicles.AddRangeAsync(
            new Vehicle { CustomerId = customerId, VehicleBrand = brand1, Model = "X5", Vin = "VIN789", RegistrationNumber = "REG789", ProductionYear = 2019, Mileage = 70000, CreatedAt = DateTime.UtcNow },
            new Vehicle { CustomerId = customerId, VehicleBrand = brand2, Model = "C-Class", Vin = "VIN012", RegistrationNumber = "REG012", ProductionYear = 2022, Mileage = 15000, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new Vehicle { CustomerId = 2, VehicleBrand = brand1, Model = "X3", Vin = "VIN345", RegistrationNumber = "REG345", ProductionYear = 2018, Mileage = 90000, CreatedAt = DateTime.UtcNow.AddDays(-2) }
        );
        await _dbContext.SaveChangesAsync();

        var result = await _vehicleService.GetCustomerVehiclesAsync(customerId);

        Xunit.Assert.NotNull(result);
        Xunit.Assert.Equal(2, result.Count);
        Xunit.Assert.Contains(result, v => v.Model == "X5");
        Xunit.Assert.Contains(result, v => v.Model == "C-Class");
        Xunit.Assert.DoesNotContain(result, v => v.Model == "X3");
    }

    [Fact]
    public async Task GetAllVehiclesAsync_ReturnsAllVehicles()
    {
        var brand1 = new VehicleBrand { Name = "audi" };
        var brand2 = new VehicleBrand { Name = "ford" };
        await _dbContext.VehicleBrands.AddRangeAsync(brand1, brand2);
        await _dbContext.Vehicles.AddRangeAsync(
            new Vehicle { Id = 1, CustomerId = 1, VehicleBrand = brand1, Model = "A4", Vin = "VIN1", RegistrationNumber = "REG1", ProductionYear = 2017, Mileage = 60000, ImageUrl = "/uploads/audi.png", CreatedAt = DateTime.UtcNow },
            new Vehicle { Id = 2, CustomerId = 2, VehicleBrand = brand2, Model = "Focus", Vin = "VIN2", RegistrationNumber = "REG2", ProductionYear = 2015, Mileage = 80000, ImageUrl = "/uploads/ford.png", CreatedAt = DateTime.UtcNow.AddDays(-5) }
        );
        await _dbContext.SaveChangesAsync();

        var result = await _vehicleService.GetAllVehiclesAsync();

        Xunit.Assert.NotNull(result);
        Xunit.Assert.Equal(2, result.Count);
        Xunit.Assert.Contains(result, v => v.Id == 1 && v.BrandName == "audi");
        Xunit.Assert.Contains(result, v => v.Id == 2 && v.BrandName == "ford");
    }

    [Fact]
    public async Task GetEditVehicleAsync_ExistingVehicle_ReturnsViewModel()
    {
        var brand = new VehicleBrand { Name = "porsche" };
        await _dbContext.VehicleBrands.AddAsync(brand);
        var vehicle = new Vehicle { Id = 10, CustomerId = 1, VehicleBrand = brand, Model = "911", Vin = "VIN10", RegistrationNumber = "REG10", ProductionYear = 2023, Mileage = 1000, CreatedAt = DateTime.UtcNow };
        await _dbContext.Vehicles.AddAsync(vehicle);
        await _dbContext.SaveChangesAsync();

        var result = await _vehicleService.GetEditVehicleAsync(10);

        Xunit.Assert.NotNull(result);
        Xunit.Assert.Equal(10, result.Id);
        Xunit.Assert.Equal("porsche", result.Brand);
        Xunit.Assert.Equal("911", result.Model);
    }

    [Fact]
    public async Task GetEditVehicleAsync_NonExistingVehicle_ReturnsNull()
    {
        var result = await _vehicleService.GetEditVehicleAsync(999);

        Xunit.Assert.Null(result);
    }

    [Fact]
    public async Task UpdateVehicleAsync_UpdatesVehicleDetailsAndBrand()
    {
        var existingBrand = new VehicleBrand { Name = "oldbrand" };
        var newBrand = new VehicleBrand { Name = "newbrand" }; // Mapper will create if not exists
        await _dbContext.VehicleBrands.AddAsync(existingBrand);
        var vehicle = new Vehicle { Id = 1, CustomerId = 1, VehicleBrand = existingBrand, Model = "OldModel", Vin = "OldVIN", RegistrationNumber = "OldREG", ProductionYear = 2000, Mileage = 100000, CreatedAt = DateTime.UtcNow };
        await _dbContext.Vehicles.AddAsync(vehicle);
        await _dbContext.SaveChangesAsync();

        var editViewModel = new VehicleEditViewModel
        {
            Id = 1,
            Brand = "NewBrand",
            Model = "NewModel",
            Vin = "NewVIN",
            RegistrationNumber = "NewREG",
            Year = 2024,
            Mileage = 5000
        };

        await _vehicleService.UpdateVehicleAsync(editViewModel);

        var updatedVehicle = await _dbContext.Vehicles.Include(v => v.VehicleBrand).FirstOrDefaultAsync(v => v.Id == 1);
        Xunit.Assert.NotNull(updatedVehicle);
        Xunit.Assert.Equal("newbrand", updatedVehicle.VehicleBrand.Name);
        Xunit.Assert.Equal("NewModel", updatedVehicle.Model);
        Xunit.Assert.Equal("NewVIN", updatedVehicle.Vin);
        Xunit.Assert.Equal("NewREG", updatedVehicle.RegistrationNumber);
        Xunit.Assert.Equal(2024, updatedVehicle.ProductionYear);
        Xunit.Assert.Equal(5000, updatedVehicle.Mileage);

        var newBrandCount = await _dbContext.VehicleBrands.CountAsync(b => b.Name == "newbrand");
        Xunit.Assert.Equal(1, newBrandCount);
    }

    [Fact]
    public async Task UpdateVehicleAsync_NonExistingVehicle_DoesNothing()
    {
        var editViewModel = new VehicleEditViewModel
        {
            Id = 999,
            Brand = "Brand",
            Model = "Model",
            Vin = "VIN",
            RegistrationNumber = "REG",
            Year = 2020,
            Mileage = 10000
        };

        await _vehicleService.UpdateVehicleAsync(editViewModel);

        var vehiclesCount = await _dbContext.Vehicles.CountAsync();
        Xunit.Assert.Equal(0, vehiclesCount); // No changes expected
    }

    [Theory]
    [InlineData(".png")]
    [InlineData(".jpg")]
    [InlineData(".jpeg")]
    public async Task UploadVehiclePhotoAsync_ValidFile_UploadsPhoto(string extension)
    {
        var vehicle = new Vehicle { Id = 1, CustomerId = 1, Model = "Test", VehicleBrand = new VehicleBrand { Name = "test" }, CreatedAt = DateTime.UtcNow };
        await _dbContext.Vehicles.AddAsync(vehicle);
        await _dbContext.VehicleBrands.AddAsync(vehicle.VehicleBrand);
        await _dbContext.SaveChangesAsync();

        var content = "dummy image data";
        var fileName = $"testimage{extension}";
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;

        var formFile = new FormFile(stream, 0, stream.Length, "image", fileName);

        var uploadsPath = Path.Combine(_mockWebHostEnvironment.Object.WebRootPath, "uploads");
        if (Directory.Exists(uploadsPath))
        {
            Directory.Delete(uploadsPath, true);
        }
        Directory.CreateDirectory(uploadsPath);

        await _vehicleService.UploadVehiclePhotoAsync(1, formFile);

        var updatedVehicle = await _dbContext.Vehicles.FindAsync(1);
        Xunit.Assert.NotNull(updatedVehicle);
        Xunit.Assert.StartsWith("/uploads/", updatedVehicle.ImageUrl);
        Xunit.Assert.Contains(extension, updatedVehicle.ImageUrl);

        var savedFilePath = Path.Combine(_mockWebHostEnvironment.Object.WebRootPath, updatedVehicle.ImageUrl!.TrimStart('/'));
        Xunit.Assert.True(File.Exists(savedFilePath));
        using (var fileStream = new FileStream(savedFilePath, FileMode.Open, FileAccess.Read))
        using (var reader = new StreamReader(fileStream))
        {
            Xunit.Assert.Equal(content, await reader.ReadToEndAsync());
        }

        if (Directory.Exists(_mockWebHostEnvironment.Object.WebRootPath))
        {
            Directory.Delete(_mockWebHostEnvironment.Object.WebRootPath, true);
        }
    }

    [Theory]
    [InlineData(".gif")]
    [InlineData(".txt")]
    [InlineData(".pdf")]
    public async Task UploadVehiclePhotoAsync_InvalidFileExtension_DoesNotUploadPhoto(string extension)
    {
        var vehicle = new Vehicle { Id = 1, CustomerId = 1, Model = "Test", VehicleBrand = new VehicleBrand { Name = "test" }, CreatedAt = DateTime.UtcNow };
        await _dbContext.Vehicles.AddAsync(vehicle);
        await _dbContext.VehicleBrands.AddAsync(vehicle.VehicleBrand);
        await _dbContext.SaveChangesAsync();

        var content = "dummy image data";
        var fileName = $"testimage{extension}";
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;

        var formFile = new FormFile(stream, 0, stream.Length, "image", fileName);

        await _vehicleService.UploadVehiclePhotoAsync(1, formFile);

        var updatedVehicle = await _dbContext.Vehicles.FindAsync(1);
        Xunit.Assert.NotNull(updatedVehicle);
        Xunit.Assert.Null(updatedVehicle.ImageUrl); // ImageUrl should remain null
    }

    [Fact]
    public async Task UploadVehiclePhotoAsync_NullFile_DoesNotUploadPhoto()
    {
        var vehicle = new Vehicle { Id = 1, CustomerId = 1, Model = "Test", VehicleBrand = new VehicleBrand { Name = "test" }, CreatedAt = DateTime.UtcNow };
        await _dbContext.Vehicles.AddAsync(vehicle);
        await _dbContext.VehicleBrands.AddAsync(vehicle.VehicleBrand);
        await _dbContext.SaveChangesAsync();

        await _vehicleService.UploadVehiclePhotoAsync(1, null!);

        var updatedVehicle = await _dbContext.Vehicles.FindAsync(1);
        Xunit.Assert.NotNull(updatedVehicle);
        Xunit.Assert.Null(updatedVehicle.ImageUrl);
    }

    [Fact]
    public async Task UploadVehiclePhotoAsync_NonExistingVehicle_DoesNotUploadPhoto()
    {
        var content = "dummy image data";
        var fileName = "testimage.png";
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;

        var formFile = new FormFile(stream, 0, stream.Length, "image", fileName);

        await _vehicleService.UploadVehiclePhotoAsync(999, formFile);

        var uploadsPath = Path.Combine(_mockWebHostEnvironment.Object.WebRootPath, "uploads");
        Xunit.Assert.False(Directory.Exists(uploadsPath) && Directory.EnumerateFiles(uploadsPath).Any());
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();

        var webRootPath = _mockWebHostEnvironment.Object.WebRootPath;
        if (Directory.Exists(webRootPath))
        {
            Directory.Delete(webRootPath, true);
        }
    }
}
