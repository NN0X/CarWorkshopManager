using CarWorkshopManager.Data;
using CarWorkshopManager.Mappers;
using CarWorkshopManager.Services.Implementations;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.Customer;
using CarWorkshopManager.ViewModels.Vehicle;   
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace CarWorkshopManager.Tests.Services.Implementations
{
    public class CustomerServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _db;
        private readonly CustomerService _service;
        private readonly Mock<IVehicleService> _vehicleServiceMock;

        public CustomerServiceTests()
        {
            var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db  = new ApplicationDbContext(opts);
            _vehicleServiceMock = new Mock<IVehicleService>();
            var mapper = new CustomerMapper();

            _service = new CustomerService(_db, mapper, _vehicleServiceMock.Object, NullLogger<CustomerService>.Instance);
        }
        public void Dispose() => _db.Dispose();
        
        [Fact]
        public async Task AddCustomerAsync_PersistsCustomer()
        {
            var vm = new CreateCustomerViewModel
            {
                FirstName = "Jan",
                LastName = "Kowalski",
                Email = "jan@example.com",
                PhoneNumber = "123456789"
            };

            await _service.AddCustomerAsync(vm);

            var saved = await _db.Customers.SingleAsync();
            Assert.Equal("Jan", saved.FirstName);
            Assert.Equal("Kowalski", saved.LastName);
            Assert.Equal("jan@example.com", saved.Email);
            Assert.Equal("123456789", saved.PhoneNumber);
        }
        
        [Fact]
        public async Task GetAllCustomersAsync_ReturnsAllMapped()
        {
            _db.Customers.AddRange(new[]
            {
                new Models.Domain.Customer { FirstName = "A", LastName = "A1", Email = "a@a.pl", PhoneNumber = "1" },
                new Models.Domain.Customer { FirstName = "B", LastName = "B1", Email = "b@b.pl", PhoneNumber = "2" }
            });
            await _db.SaveChangesAsync();

            var list = await _service.GetAllCustomersAsync();

            Assert.Equal(2, list.Count);
            Assert.Contains(list, c => c.FirstName == "A" && c.LastName == "A1");
            Assert.Contains(list, c => c.FirstName == "B" && c.LastName == "B1");
        }
        
        [Fact]
        public async Task GetCustomerAsync_NotFound_ReturnsNull()
        {
            var result = await _service.GetCustomerAsync(999);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCustomerAsync_Found_ReturnsViewModel()
        {
            var cust = new Models.Domain.Customer
            {
                FirstName = "X",
                LastName = "Y",
                Email = "x@y.pl",
                PhoneNumber = "555"
            };
            
            _db.Customers.Add(cust);
            await _db.SaveChangesAsync();

            var vm = await _service.GetCustomerAsync(cust.Id);

            Assert.NotNull(vm);
            Assert.Equal(cust.Id, vm!.Id);
            Assert.Equal("X", vm.FirstName);
            Assert.Equal("Y", vm.LastName);
            Assert.Equal("x@y.pl", vm.Email);
            Assert.Equal("555", vm.PhoneNumber);
        }
        
        [Fact]
        public async Task GetCustomerDetailsAsync_NotFound_ReturnsNull()
        {
            var result = await _service.GetCustomerDetailsAsync(42);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCustomerDetailsAsync_Found_ReturnsDetailsWithVehicles()
        {
            var cust = new Models.Domain.Customer
            {
                FirstName = "C",
                LastName = "D",
                Email = "c@d.pl",
                PhoneNumber = "888"
            };
            _db.Customers.Add(cust);
            await _db.SaveChangesAsync();
            
            var fakeVehicles = new List<VehicleListItemViewModel>
            {
                new VehicleListItemViewModel { Id = 1, Model = "M1" },
                new VehicleListItemViewModel { Id = 2, Model = "M2" }
            };
            
            _vehicleServiceMock
                .Setup(v => v.GetCustomerVehiclesAsync(cust.Id))
                .ReturnsAsync(fakeVehicles);

            var details = await _service.GetCustomerDetailsAsync(cust.Id);

            Assert.NotNull(details);
            Assert.Equal(cust.Id, details!.Id);
            Assert.Equal(2, details.Vehicles.Count);
            Assert.Equal("M1", details.Vehicles[0].Model);
            Assert.Equal(1, details.Vehicles[0].Id);
        }
    }
}
