using System.Text;
using CarWorkshopManager.Data;
using CarWorkshopManager.Mappers;
using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.Services.Implementations;
using CarWorkshopManager.ViewModels.Vehicle;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace CarWorkshopManager.Tests.Services.Implementations
{
    public class VehicleServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _db;
        private readonly string _tempRoot;
        private readonly VehicleService _service;

        public VehicleServiceTests()
        {
            var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(opts);

            _tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempRoot);

            var envMock = new Mock<IWebHostEnvironment>();
            envMock.SetupGet(e => e.WebRootPath).Returns(_tempRoot);

            var mapper = new VehicleMapper();
            _service = new VehicleService(_db, envMock.Object, mapper);
        }

        public void Dispose()
        {
            _db.Dispose();
            if (Directory.Exists(_tempRoot)) Directory.Delete(_tempRoot, true);
        }

        [Fact]
        public async Task AddVehicleAsync_AddsAndCreatesBrandIfMissing()
        {
            var vm = new AddVehicleViewModel
            {
                Brand = "Toyota",
                Model = "Corolla",
                RegistrationNumber = "AB123",
                Vin = "VIN123",
                CustomerId = 1
            };

            await _service.AddVehicleAsync(vm);

            var v = await _db.Vehicles.Include(x => x.VehicleBrand).SingleAsync();
            Assert.Equal("toyota", v.VehicleBrand.Name);
            Assert.Equal("Corolla", v.Model);
        }

        [Fact]
        public async Task GetCustomerVehiclesAsync_ReturnsOnlyCustomerCars()
        {
            _db.Vehicles.AddRange(
                new Vehicle { CustomerId = 1, Model = "M1", VehicleBrand = new VehicleBrand { Name = "A" } },
                new Vehicle { CustomerId = 2, Model = "M2", VehicleBrand = new VehicleBrand { Name = "B" } });
            await _db.SaveChangesAsync();

            var list = await _service.GetCustomerVehiclesAsync(1);

            Assert.Single(list);
            Assert.Equal("M1", list[0].Model);
        }

        [Fact]
        public async Task GetEditVehicleAsync_ReturnsMappedViewModel()
        {
            var v = new Vehicle
            {
                Model = "Focus",
                VehicleBrand = new VehicleBrand { Name = "Ford" }
            };
            _db.Vehicles.Add(v);
            await _db.SaveChangesAsync();

            var vm = await _service.GetEditVehicleAsync(v.Id);

            Assert.NotNull(vm);
            Assert.Equal("Ford", vm!.Brand, ignoreCase: true);
            Assert.Equal("Focus", vm.Model);
        }

        [Fact]
        public async Task UpdateVehicleAsync_UpdatesDataAndBrand()
        {
            var v = new Vehicle
            {
                Model = "Old",
                VehicleBrand = new VehicleBrand { Name = "Opel" }
            };
            _db.Vehicles.Add(v);
            await _db.SaveChangesAsync();

            var vm = new VehicleEditViewModel
            {
                Id = v.Id,
                Brand = "Honda",
                Model = "Civic"
            };

            await _service.UpdateVehicleAsync(vm);

            Assert.Equal("Civic", v.Model);
            Assert.Equal("honda", v.VehicleBrand.Name);
        }

        [Fact]
        public async Task UploadVehiclePhotoAsync_SavesFileAndSetsUrl()
        {
            var v = new Vehicle { Model = "Pic" };
            _db.Vehicles.Add(v);
            await _db.SaveChangesAsync();

            var bytes = Encoding.UTF8.GetBytes("fake-img");
            await using var ms = new MemoryStream(bytes);
            IFormFile formFile = new FormFile(ms, 0, bytes.Length, "Data", "photo.jpg");

            await _service.UploadVehiclePhotoAsync(v.Id, formFile);

            Assert.StartsWith("/uploads/", v.ImageUrl);
            var savedPath = Path.Combine(_tempRoot, v.ImageUrl.TrimStart('/'));
            Assert.True(File.Exists(savedPath));
        }
    }
}
