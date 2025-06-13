using CarWorkshopManager.Constants;
using CarWorkshopManager.Data;
using CarWorkshopManager.Mappers;
using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.Models.Identity;
using CarWorkshopManager.Services.Implementations;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.ServiceOrder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace CarWorkshopManager.Tests.Services.Implementations
{
    public class ServiceOrderServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _db;
        private readonly ServiceOrderService _service;
        private readonly string _adminId = "admin";
        private readonly string _mechanicId = "mech";
        private readonly string _plainUserId = "user";

        public ServiceOrderServiceTests()
        {
            var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(opts);

            SeedStaticLookups();
            SeedUsers();

            var mapper = new ServiceOrderMapper(); 
            var workRateSvcMock = new Mock<IWorkRateService>();
            var partSvcMock = new Mock<IPartService>();
            var userManagerMock = MockUserManager();

            _service = new ServiceOrderService(_db, mapper, workRateSvcMock.Object, partSvcMock.Object, 
                userManagerMock.Object, NullLogger<ServiceOrderService>.Instance);
        }

        private static Mock<UserManager<ApplicationUser>> MockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        private void SeedStaticLookups()
        {
            _db.OrderStatuses.AddRange(
                new OrderStatus { Id = 1, Name = OrderStatuses.New },
                new OrderStatus { Id = 2, Name = OrderStatuses.InProgress },
                new OrderStatus { Id = 3, Name = OrderStatuses.Completed }
            );
            _db.Roles.Add(new IdentityRole { Id = "adminRole", Name = Roles.Admin, NormalizedName = Roles.Admin.ToUpper() });
            _db.SaveChanges();
        }

        private void SeedUsers()
        {
            _db.Users.AddRange(
                new ApplicationUser { Id = _adminId, UserName = "admin@test" },
                new ApplicationUser { Id = _mechanicId, UserName = "mech@test" },
                new ApplicationUser { Id = _plainUserId, UserName = "user@test" }
            );
            _db.UserRoles.Add(new IdentityUserRole<string> { UserId = _adminId, RoleId = "adminRole" });
            _db.SaveChanges();
        }

        public void Dispose() => _db.Dispose();

        private Vehicle AddVehicle()
        {
            var cust = new Customer { FirstName = "John", LastName = "Doe" };
            var veh  = new Vehicle { RegistrationNumber = "ABC123", Vin = "VINVINVIN", Customer = cust };
            _db.Vehicles.Add(veh);
            _db.SaveChanges();
            return veh;
        }
        
        [Fact]
        public async Task CreateOrderAsync_SavesOrderAndReturnsId()
        {
            var vehicle = AddVehicle();
            var model = new CreateServiceOrderViewModel { VehicleId = vehicle.Id, Description = "Fix brakes" };

            var newId = await _service.CreateOrderAsync(model, _plainUserId);

            var saved = await _db.ServiceOrders.FindAsync(newId);
            Assert.NotNull(saved);
            Assert.Equal(_plainUserId, saved!.CreatedById);
            Assert.Equal(vehicle.Id, saved.VehicleId);
            Assert.Equal("John Doe", saved.CustomerNameSnapshot);
            Assert.Equal("ABC123", saved.RegistrationNumberSnapshot);
            Assert.StartsWith("ORD-", saved.OrderNumber);
            Assert.Equal(1, saved.StatusId); 
        }
        
        [Fact]
        public async Task ChangeStatusAsync_AdminCanCompleteOrder()
        {
            var vehicle = AddVehicle();
            var order = new ServiceOrder { VehicleId = vehicle.Id, StatusId = 1, OpenedAt = DateTime.UtcNow };
            _db.ServiceOrders.Add(order);
            await _db.SaveChangesAsync();

            var ok = await _service.ChangeStatusAsync(order.Id, OrderStatuses.Completed, _adminId);

            Assert.True(ok);
            Assert.Equal(3, order.StatusId);
            Assert.NotNull(order.ClosedAt);
        }

        [Fact]
        public async Task ChangeStatusAsync_MechanicOnTaskCanChange()
        {
            var vehicle = AddVehicle();
            var order = new ServiceOrder { VehicleId = vehicle.Id, StatusId = 1, OpenedAt = DateTime.UtcNow };
            var task = new ServiceTask { ServiceOrder = order };
            order.Tasks.Add(task);
            var mech = await _db.Users.FindAsync(_mechanicId);
            task.Mechanics.Add(mech!);

            _db.ServiceOrders.Add(order);
            await _db.SaveChangesAsync();

            var ok = await _service.ChangeStatusAsync(order.Id, OrderStatuses.InProgress, _mechanicId);

            Assert.True(ok);
            Assert.Equal(2, order.StatusId);
        }

        [Fact]
        public async Task ChangeStatusAsync_NotParticipantFails()
        {
            var vehicle = AddVehicle();
            var order = new ServiceOrder { VehicleId = vehicle.Id, StatusId = 1, OpenedAt = DateTime.UtcNow };
            _db.ServiceOrders.Add(order);
            await _db.SaveChangesAsync();

            var ok = await _service.ChangeStatusAsync(order.Id, OrderStatuses.InProgress, _plainUserId);
            Assert.False(ok);
        }
        
        [Fact]
        public async Task GetOpenServiceOrdersAsync_ReturnsNonCompleted()
        {
            var vehicle = AddVehicle();
            _db.ServiceOrders.AddRange(
                new ServiceOrder { VehicleId = vehicle.Id, StatusId = 1 }, 
                new ServiceOrder { VehicleId = vehicle.Id, StatusId = 3 }  
            );
            await _db.SaveChangesAsync();

            var list = await _service.GetOpenServiceOrdersAsync();

            Assert.Single(list);
            Assert.Equal(1, list[0].StatusName == OrderStatuses.Completed ? 0 : 1); 
        }

        [Fact]
        public async Task GetOrderTotalsAsync_ComputesAggregates()
        {
            var vehicle = AddVehicle();
            var order   = new ServiceOrder { VehicleId = vehicle.Id, StatusId = 1 };

            var task1 = new ServiceTask { TotalNet = 100, TotalVat = 23 };
            var task2 = new ServiceTask { TotalNet = 50,  TotalVat = 11.5m };

            task1.UsedParts.Add(new UsedPart { TotalNet = 30, TotalVat = 6.9m });
            task2.UsedParts.Add(new UsedPart { TotalNet = 20, TotalVat = 4.6m });

            order.Tasks.Add(task1);
            order.Tasks.Add(task2);

            _db.ServiceOrders.Add(order);
            await _db.SaveChangesAsync();

            var (laborNet, laborVat, partsNet, partsVat) = await _service.GetOrderTotalsAsync(order.Id);

            Assert.Equal(150, laborNet);
            Assert.Equal(34.5m, laborVat);
            Assert.Equal(50, partsNet);
            Assert.Equal(11.5m, partsVat);
        }
    }
}
