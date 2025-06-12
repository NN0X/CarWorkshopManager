using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CarWorkshopManager.Constants;
using CarWorkshopManager.Data;
using CarWorkshopManager.Mappers;
using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.Models.Identity;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.ServiceOrder;
using CarWorkshopManager.ViewModels.ServiceTasks;
using CarWorkshopManager.ViewModels.UsedPart;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CarWorkshopManager.Services.Implementations
{
    public class ServiceOrderService : IServiceOrderService
    {
        private readonly ApplicationDbContext _db;
        private readonly ServiceOrderMapper _mapper;
        private readonly IWorkRateService _workRateService;
        private readonly IPartService _partService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ServiceOrderService(
            ApplicationDbContext db,
            ServiceOrderMapper mapper,
            IWorkRateService workRateService,
            IPartService partService,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _mapper = mapper;
            _workRateService = workRateService;
            _partService = partService;
            _userManager = userManager;
        }

        public async Task<List<ServiceOrderListItemViewModel>> GetAllServiceOrdersAsync()
        {
            var orders = await _db.ServiceOrders
                .Include(o => o.Status)
                .ToListAsync();

            return orders.Select(o => _mapper.ToServiceOrderListItemViewModel(o)).ToList();
        }

        public async Task<int> CreateOrderAsync(CreateServiceOrderViewModel model, string userId)
        {
            var vehicle = await _db.Vehicles.Include(v => v.Customer)
                .FirstOrDefaultAsync(v => v.Id == model.VehicleId)
                ?? throw new Exception("Pojazd nie istnieje");

            var statusId = await _db.OrderStatuses
                .Where(s => s.Name == OrderStatuses.New)
                .Select(s => s.Id)
                .FirstAsync();

            var order = _mapper.ToServiceOrder(model);
            order.CreatedById = userId;
            order.StatusId = statusId;
            order.CustomerNameSnapshot = $"{vehicle.Customer.FirstName} {vehicle.Customer.LastName}";
            order.RegistrationNumberSnapshot = vehicle.RegistrationNumber;
            order.OrderNumber = $"ORD-{Guid.NewGuid():N}".ToUpper()[..8];

            _db.ServiceOrders.Add(order);
            await _db.SaveChangesAsync();
            return order.Id;
        }

        public async Task<ServiceOrderDetailsViewModel?> GetOrderDetailsAsync(int id)
        {
            var order = await _db.ServiceOrders
                .Include(o => o.Status)
                .Include(o => o.Tasks).ThenInclude(t => t.Mechanics)
                .Include(o => o.Tasks).ThenInclude(t => t.UsedParts).ThenInclude(up => up.Part)
                .Include(o => o.Comments).ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(o => o.Id == id);

            return order == null
                ? null
                : _mapper.ToServiceOrderDetailsViewModel(order);
        }

        public async Task<bool> ChangeStatusAsync(int id, string newStatus, string userId)
        {
            var order = await _db.ServiceOrders
                .Include(o => o.Tasks).ThenInclude(t => t.Mechanics)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return false;

            var isAdmin = await _db.Users.AnyAsync(u =>
                u.Id == userId &&
                _db.UserRoles.Any(ur =>
                    ur.UserId == u.Id &&
                    _db.Roles.Any(r => r.Id == ur.RoleId && r.Name == Roles.Admin)));

            var isMechanic = order.Tasks.SelectMany(t => t.Mechanics)
                                        .Any(m => m.Id == userId);
            if (!isAdmin && !isMechanic) return false;

            var status = await _db.OrderStatuses.FirstOrDefaultAsync(s => s.Name == newStatus);
            if (status == null) return false;

            order.StatusId = status.Id;
            order.ClosedAt = newStatus == OrderStatuses.Completed
                ? DateTime.UtcNow
                : null;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task PopulateDetailsViewModelAsync(ServiceOrderDetailsViewModel vm, ClaimsPrincipal user)
        {
            vm.NewTask = new ServiceTaskFormViewModel { ServiceOrderId = vm.Id };
            vm.NewUsedPart = new UsedPartFormViewModel();
            vm.NewCommentContent = string.Empty;

            vm.WorkRates = await _workRateService.GetSelectWorkRatesAsync();
            vm.Parts = await _partService.GetActivePartsSelectAsync();
            vm.Mechanics = new SelectList(
                await _userManager.GetUsersInRoleAsync(Roles.Mechanic),
                "Id", "UserName");
            vm.Statuses = new SelectList(OrderStatuses.AllStatuses);
        }
    }
}
