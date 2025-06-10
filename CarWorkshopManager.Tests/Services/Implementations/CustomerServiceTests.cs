using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarWorkshopManager.Services.Implementations;
using CarWorkshopManager.Data;
using CarWorkshopManager.Mappers;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.Customer;
using CarWorkshopManager.Models.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using CarWorkshopManager.ViewModels.Vehicle;
using System;

public class CustomerServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<CustomerMapper> _mockMapper;
    private readonly Mock<IVehicleService> _mockVehicleService;
    private readonly CustomerService _customerService;

    public CustomerServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "CustomerServiceTestDb_" + Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);

        _mockMapper = new Mock<CustomerMapper>();
        _mockVehicleService = new Mock<IVehicleService>();
        _customerService = new CustomerService(_dbContext, _mockMapper.Object, _mockVehicleService.Object);
    }

    [Fact]
    public async Task AddCustomerAsync_AddsCustomerToDatabase()
    {
        var createCustomerViewModel = new CreateCustomerViewModel
        {
            FirstName = "Test",
            LastName = "Customer",
            Email = "test@example.com",
            PhoneNumber = "123-456-7890"
        };
        var customer = new Customer
        {
            Id = 1,
            FirstName = "Test",
            LastName = "Customer",
            Email = "test@example.com",
            PhoneNumber = "123-456-7890"
        };

        _mockMapper.Setup(m => m.ToCustomer(createCustomerViewModel))
                   .Returns(customer);

        await _customerService.AddCustomerAsync(createCustomerViewModel);

        var addedCustomer = await _dbContext.Customers.FirstOrDefaultAsync();
        Xunit.Assert.NotNull(addedCustomer);
        Xunit.Assert.Equal("Test", addedCustomer.FirstName);
        Xunit.Assert.Equal("Customer", addedCustomer.LastName);
    }

    [Fact]
    public async Task GetAllCustomersAsync_ReturnsListOfCustomerListItemViewModel()
    {
        var customers = new List<Customer>
        {
            new Customer { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", PhoneNumber = "111" },
            new Customer { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", PhoneNumber = "222" }
        };
        await _dbContext.Customers.AddRangeAsync(customers);
        await _dbContext.SaveChangesAsync();

        _mockMapper.Setup(m => m.ToCreateCustomerListItemViewModel(It.IsAny<Customer>()))
                   .Returns((Func<Customer, CustomerListItemViewModel>)((c) =>
                       new CustomerListItemViewModel
                       {
                           Id = c.Id,
                           FirstName = c.FirstName,
                           LastName = c.LastName,
                           Email = c.Email,
                           PhoneNumber = c.PhoneNumber
                       }));

        var result = await _customerService.GetAllCustomersAsync();

        Xunit.Assert.NotNull(result);
        Xunit.Assert.Equal(2, result.Count);
        Xunit.Assert.Contains(result, c => c.FirstName == "John" && c.LastName == "Doe");
        Xunit.Assert.Contains(result, c => c.FirstName == "Jane" && c.LastName == "Smith");
        Xunit.Assert.Contains(result, c => c.Email == "john@example.com");
    }

    [Fact]
    public async Task GetCustomerAsync_ExistingCustomer_ReturnsCustomerListItemViewModel()
    {
        var customer = new Customer { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", PhoneNumber = "111" };
        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        _mockMapper.Setup(m => m.ToCreateCustomerListItemViewModel(It.IsAny<Customer>()))
                   .Returns((Func<Customer, CustomerListItemViewModel>)((c) =>
                       new CustomerListItemViewModel
                       {
                           Id = c.Id,
                           FirstName = c.FirstName,
                           LastName = c.LastName,
                           Email = c.Email,
                           PhoneNumber = c.PhoneNumber
                       }));

        var result = await _customerService.GetCustomerAsync(1);

        Xunit.Assert.NotNull(result);
        Xunit.Assert.Equal(1, result.Id);
        Xunit.Assert.Equal("John", result.FirstName);
        Xunit.Assert.Equal("Doe", result.LastName);
    }

    [Fact]
    public async Task GetCustomerAsync_NonExistingCustomer_ReturnsNull()
    {
        var result = await _customerService.GetCustomerAsync(99);

        Xunit.Assert.Null(result);
    }

    [Fact]
    public async Task GetCustomerDetailsAsync_ExistingCustomer_ReturnsCustomerDetailsViewModel()
    {
        var customer = new Customer
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "111-222-3333"
        };
        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        var vehicles = new List<VehicleListItemViewModel>
        {
            new VehicleListItemViewModel { Id = 101, Model = "Golf", BrandName = "VW", Vin = "VIN001", RegistrationNumber = "REG001", ProductionYear = 2020, Mileage = 50000, CreatedAt = DateTime.Now },
            new VehicleListItemViewModel { Id = 102, Model = "Polo", BrandName = "VW", Vin = "VIN002", RegistrationNumber = "REG002", ProductionYear = 2021, Mileage = 30000, CreatedAt = DateTime.Now.AddDays(-10) }
        };
        _mockVehicleService.Setup(vs => vs.GetCustomerVehiclesAsync(1)).ReturnsAsync(vehicles);

        var result = await _customerService.GetCustomerDetailsAsync(1);

        Xunit.Assert.NotNull(result);
        Xunit.Assert.Equal(1, result.Id);
        Xunit.Assert.Equal("John", result.FirstName);
        Xunit.Assert.Equal("Doe", result.LastName);

        Xunit.Assert.Equal(vehicles.Count, result.Vehicles.Count);
        Xunit.Assert.Contains(result.Vehicles, v => v.Model == "Golf" && v.BrandName == "VW");
        Xunit.Assert.Contains(result.Vehicles, v => v.Model == "Polo" && v.BrandName == "VW");

        Xunit.Assert.True(result.Vehicles.OrderBy(v => v.Id).SequenceEqual(vehicles.OrderBy(v => v.Id), new VehicleListItemViewModelComparer()));
    }

    [Fact]
    public async Task GetCustomerDetailsAsync_NonExistingCustomer_ReturnsNull()
    {
        var result = await _customerService.GetCustomerDetailsAsync(99);

        Xunit.Assert.Null(result);
    }

    private class VehicleListItemViewModelComparer : IEqualityComparer<VehicleListItemViewModel>
    {
        public bool Equals(VehicleListItemViewModel? x, VehicleListItemViewModel? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;

            return x.Id == y.Id &&
                   x.BrandName == y.BrandName &&
                   x.Model == y.Model &&
                   x.Vin == y.Vin &&
                   x.RegistrationNumber == y.RegistrationNumber &&
                   x.ProductionYear == y.ProductionYear &&
                   x.Mileage == y.Mileage;
        }

        public int GetHashCode(VehicleListItemViewModel obj)
        {
            return HashCode.Combine(obj.Id, obj.BrandName, obj.Model, obj.Vin, obj.RegistrationNumber, obj.ProductionYear, obj.Mileage);
        }
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
