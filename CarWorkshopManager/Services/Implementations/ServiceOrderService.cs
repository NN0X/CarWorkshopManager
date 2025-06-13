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
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CarWorkshopManager.Services.Implementations
{
    public class ServiceOrderService : IServiceOrderService
    {
        private readonly ApplicationDbContext _db;
        private readonly ServiceOrderMapper _mapper;
        private readonly IWorkRateService _workRateService;
        private readonly IPartService _partService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ServiceOrderService> _logger;

        public ServiceOrderService(
            ApplicationDbContext db,
            ServiceOrderMapper mapper,
            IWorkRateService workRateService,
            IPartService partService,
            UserManager<ApplicationUser> userManager,
            ILogger<ServiceOrderService> logger)
        {
            _db = db;
            _mapper = mapper;
            _workRateService = workRateService;
            _partService = partService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<List<ServiceOrderListItemViewModel>> GetAllServiceOrdersAsync()
        {
            _logger.LogInformation("GetAllServiceOrdersAsync called");
            var orders = await _db.ServiceOrders.Include(o => o.Status).ToListAsync();
            return orders.Select(o => _mapper.ToServiceOrderListItemViewModel(o)).ToList();
        }

        public async Task<int> CreateOrderAsync(CreateServiceOrderViewModel model, string userId)
        {
            _logger.LogInformation("CreateOrderAsync called: VehicleId={VehicleId}", model.VehicleId);
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
            order.CustomerNameSnapshot     = $"{vehicle.Customer.FirstName} {vehicle.Customer.LastName}";
            order.RegistrationNumberSnapshot = vehicle.RegistrationNumber;
            order.OrderNumber = "ORD-" + Guid.NewGuid().ToString("N")[..8].ToUpper();

            _db.ServiceOrders.Add(order);
            await _db.SaveChangesAsync();
            _logger.LogInformation("CreateOrderAsync: created order Id={OrderId}", order.Id);
            return order.Id;
        }

        public async Task<ServiceOrderDetailsViewModel?> GetOrderDetailsAsync(int id)
        {
            _logger.LogInformation("GetOrderDetailsAsync called: Id={Id}", id);
            var order = await _db.ServiceOrders
                .Include(o => o.Status)
                .Include(o => o.Tasks).ThenInclude(t => t.Mechanics)
                .Include(o => o.Tasks).ThenInclude(t => t.UsedParts).ThenInclude(up => up.Part)
                .Include(o => o.Comments).ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null)
            {
                _logger.LogWarning("GetOrderDetailsAsync: not found Id={Id}", id);
                return null;
            }

            return _mapper.ToServiceOrderDetailsViewModel(order);
        }

        public async Task<bool> ChangeStatusAsync(int id, string newStatus, string userId)
        {
            _logger.LogInformation("ChangeStatusAsync called: Id={Id}, Status={Status}", id, newStatus);
            var order = await _db.ServiceOrders
                .Include(o => o.Tasks).ThenInclude(t => t.Mechanics)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order is null)
            {
                _logger.LogWarning("ChangeStatusAsync: order not found Id={Id}", id);
                return false;
            }

            var isAdmin = await _db.UserRoles.AnyAsync(ur =>
                ur.UserId == userId &&
                _db.Roles.Any(r => r.Id == ur.RoleId && r.Name == Roles.Admin));

            var isMechanic = order.Tasks
                .SelectMany(t => t.Mechanics)
                .Any(m => m.Id == userId);

            if (!isAdmin && !isMechanic)
            {
                _logger.LogWarning("ChangeStatusAsync: unauthorized user {UserId}", userId);
                return false;
            }

            var status = await _db.OrderStatuses.FirstOrDefaultAsync(s => s.Name == newStatus);
            if (status is null)
            {
                _logger.LogWarning("ChangeStatusAsync: invalid status {Status}", newStatus);
                return false;
            }

            order.StatusId = status.Id;
            order.ClosedAt = newStatus == OrderStatuses.Completed
                ? DateTime.UtcNow
                : null;

            await _db.SaveChangesAsync();
            _logger.LogInformation("ChangeStatusAsync: status changed for {Id}", id);
            return true;
        }

        public async Task PopulateDetailsViewModelAsync(ServiceOrderDetailsViewModel vm, ClaimsPrincipal user)
        {
            _logger.LogInformation("PopulateDetailsViewModelAsync called: Id={Id}", vm.Id);
            vm.NewTask = new ServiceTaskFormViewModel { ServiceOrderId = vm.Id };
            vm.NewUsedPart = new UsedPartFormViewModel();
            vm.NewCommentContent = string.Empty;
            vm.WorkRates = await _workRateService.GetSelectWorkRatesAsync();
            vm.Parts     = await _partService.GetActivePartsSelectAsync();
            vm.Mechanics = new SelectList(
                await _userManager.GetUsersInRoleAsync(Roles.Mechanic),
                "Id", "UserName");
            var statusItems = new[]
            {
                new { Value = OrderStatuses.New, Text = "Nowe" },
                new { Value = OrderStatuses.InProgress, Text = "W trakcie" },
                new { Value = OrderStatuses.Completed, Text = "Zakończone" }
            };
            vm.Statuses = new SelectList(statusItems, "Value", "Text", vm.StatusName);
        }

        public async Task<RepairCostReportViewModel> GetRepairCostReportAsync(DateTime? month, int? vehicleId)
        {
            _logger.LogInformation("GetRepairCostReportAsync called: month={Month}, vehicle={Vehicle}", month, vehicleId);
            var query = _db.ServiceOrders
                .Include(o => o.Tasks).ThenInclude(t => t.UsedParts)
                .AsQueryable();

            if (vehicleId.HasValue)
                query = query.Where(o => o.VehicleId == vehicleId.Value);

            if (month.HasValue)
            {
                var from = new DateTime(month.Value.Year, month.Value.Month, 1);
                var to   = from.AddMonths(1);
                query = query.Where(o => o.OpenedAt >= from && o.OpenedAt < to);
            }

            var orders = await query.ToListAsync();
            var items = orders.Select(o =>
            {
                var laborNet = o.Tasks.Sum(t => t.TotalNet);
                var partsNet = o.Tasks.SelectMany(t => t.UsedParts).Sum(up => up.TotalNet);
                var totalVat = o.Tasks.Sum(t => t.TotalVat) + o.Tasks.SelectMany(t => t.UsedParts).Sum(up => up.TotalVat);
                return new RepairCostReportItemViewModel
                {
                    ServiceOrderId = o.Id,
                    OrderNumber = o.OrderNumber,
                    OpenedAt = o.OpenedAt,
                    CustomerName = o.CustomerNameSnapshot,
                    RegistrationNumber = o.RegistrationNumberSnapshot,
                    LaborNet = laborNet,
                    PartsNet = partsNet,
                    TotalVat = totalVat
                };
            }).ToList();

            var vehicles = await _db.Vehicles
                .Select(v => new SelectListItem { Value = v.Id.ToString(), Text = $"{v.RegistrationNumber} ({v.Vin})" })
                .ToListAsync();

            return new RepairCostReportViewModel
            {
                Month = month,
                VehicleId = vehicleId,
                Vehicles  = new SelectList(vehicles, "Value", "Text", vehicleId),
                Items     = items
            };
        }

        public async Task<MonthlyRepairSummaryReportViewModel> GetMonthlyRepairSummaryAsync(DateTime month)
        {
            _logger.LogInformation("GetMonthlyRepairSummaryAsync called for {Month}", month);
            var from = new DateTime(month.Year, month.Month, 1);
            var to   = from.AddMonths(1);
            var data = await _db.ServiceOrders
                .Where(o => o.OpenedAt >= from && o.OpenedAt < to && o.Status.Name != OrderStatuses.Completed)
                .Select(o => new
                {
                    o.CustomerNameSnapshot,
                    o.RegistrationNumberSnapshot,
                    LaborNet = o.Tasks.Sum(t => t.TotalNet),
                    PartsNet = o.Tasks.SelectMany(t => t.UsedParts).Sum(up => up.TotalNet),
                    LaborVat = o.Tasks.Sum(t => t.TotalVat),
                    PartsVat = o.Tasks.SelectMany(t => t.UsedParts).Sum(up => up.TotalVat)
                })
                .ToListAsync();

            var summary = data.GroupBy(x => (x.CustomerNameSnapshot, x.RegistrationNumberSnapshot))
                .Select(g => new MonthlyRepairSummaryItemViewModel
                {
                    CustomerName = g.Key.CustomerNameSnapshot,
                    RegistrationNumber = g.Key.RegistrationNumberSnapshot,
                    OrdersCount = g.Count(),
                    TotalCostNet = g.Sum(x => x.LaborNet + x.PartsNet),
                    TotalVat = g.Sum(x => x.LaborVat + x.PartsVat)
                })
                .OrderBy(x => x.CustomerName)
                .ToList();

            return new MonthlyRepairSummaryReportViewModel { Month = from, Items = summary };
        }

        public async Task<List<ServiceOrderListItemViewModel>> GetOpenServiceOrdersAsync()
        {
            _logger.LogInformation("GetOpenServiceOrdersAsync called");
            var orders = await _db.ServiceOrders
                .Include(o => o.Status)
                .Where(o => o.Status.Name != OrderStatuses.Completed)
                .ToListAsync();
            return orders.Select(o => _mapper.ToServiceOrderListItemViewModel(o)).ToList();
        }

        public async Task<(decimal laborNet, decimal laborVat, decimal partsNet, decimal partsVat)> GetOrderTotalsAsync(int orderId)
        {
            _logger.LogInformation("GetOrderTotalsAsync called for {OrderId}", orderId);
            var rows = await _db.ServiceTasks
                .Where(t => t.ServiceOrderId == orderId)
                .Select(t => new
                {
                    t.TotalNet,
                    t.TotalVat,
                    PartsNet = t.UsedParts.Sum(up => up.TotalNet),
                    PartsVat = t.UsedParts.Sum(up => up.TotalVat)
                })
                .ToListAsync();

            return (rows.Sum(r => r.TotalNet),
                    rows.Sum(r => r.TotalVat),
                    rows.Sum(r => r.PartsNet),
                    rows.Sum(r => r.PartsVat));
        }
    }
}
