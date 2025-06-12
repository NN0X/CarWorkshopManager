using CarWorkshopManager.Constants;
using CarWorkshopManager.Data;
using CarWorkshopManager.Mappers;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.ServiceOrder;
using Microsoft.EntityFrameworkCore;
using CarWorkshopManager.Models.Domain;

namespace CarWorkshopManager.Services.Implementations
{
    public class ServiceOrderService : IServiceOrderService
    {
        private readonly ApplicationDbContext _db;
        private readonly ServiceOrderMapper _mapper;

        public ServiceOrderService(ApplicationDbContext db, ServiceOrderMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<ServiceOrderListItemViewModel>> GetAllServiceOrdersAsync()
        {
            var orders = await _db.ServiceOrders
                .Include(o => o.Status)
                .ToListAsync();

            return orders
                .Select(o => _mapper.ToServiceOrderListItemViewModel(o))
                .ToList();
        }

        public async Task<int> CreateOrderAsync(CreateServiceOrderViewModel model, string userId)
        {
            var vehicle = await _db.Vehicles
                .Include(v => v.Customer)
                .FirstOrDefaultAsync(v => v.Id == model.VehicleId)
                ?? throw new Exception("Pojazd nie istnieje");

            var statusId = await _db.OrderStatuses
                .Where(s => s.Name == OrderStatuses.New)
                .Select(s => s.Id)
                .FirstAsync();

            var serviceOrder = _mapper.ToServiceOrder(model);
            serviceOrder.CreatedById = userId;
            serviceOrder.StatusId = statusId;
            serviceOrder.CustomerNameSnapshot = $"{vehicle.Customer.FirstName} {vehicle.Customer.LastName}";
            serviceOrder.RegistrationNumberSnapshot = vehicle.RegistrationNumber;
            serviceOrder.OrderNumber = $"ORD-{Guid.NewGuid():N}".ToUpper()[..8];

            _db.ServiceOrders.Add(serviceOrder);
            await _db.SaveChangesAsync();

            return serviceOrder.Id;
        }

        public async Task<ServiceOrderDetailsViewModel?> GetOrderDetailsAsync(int id)
        {
            var order = await _db.ServiceOrders
                .Include(o => o.Status)
                .Include(o => o.Tasks)
                    .ThenInclude(t => t.Mechanics)
                .Include(o => o.Tasks)
                    .ThenInclude(t => t.UsedParts)
                        .ThenInclude(up => up.Part)
                .Include(o => o.Comments)
                    .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(o => o.Id == id);

            return order is null
                ? null
                : _mapper.ToServiceOrderDetailsViewModel(order);
        }

        public async Task<bool> ChangeStatusAsync(int id, string newStatus, string userId)
        {
            var order = await _db.ServiceOrders
                .Include(o => o.Tasks).ThenInclude(t => t.Mechanics)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null) return false;

            var isAdmin = await _db.Users
                .AnyAsync(u => u.Id == userId
                    && _db.UserRoles.Any(ur => ur.UserId == u.Id
                        && _db.Roles.Any(r => r.Id == ur.RoleId && r.Name == Roles.Admin)));

            var isMechanicOfOrder = order.Tasks
                .SelectMany(t => t.Mechanics)
                .Any(m => m.Id == userId);

            if (!isAdmin && !isMechanicOfOrder)
                return false;

            var status = await _db.OrderStatuses
                .FirstOrDefaultAsync(s => s.Name == newStatus);

            if (status is null)
                return false;

            order.StatusId = status.Id;
            order.ClosedAt = newStatus == OrderStatuses.Completed
                ? DateTime.UtcNow
                : null;

            await _db.SaveChangesAsync();
            return true;
        }
    }
}
