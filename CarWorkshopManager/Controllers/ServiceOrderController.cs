using System.Security.Claims;
using CarWorkshopManager.Constants;
using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.Models.Identity;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.ServiceOrder;
using CarWorkshopManager.ViewModels.ServiceTasks;
using CarWorkshopManager.ViewModels.UsedPart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CarWorkshopManager.Data;

namespace CarWorkshopManager.Controllers;

[Authorize(Roles = $"{Roles.Receptionist},{Roles.Admin},{Roles.Mechanic}")]
public class ServiceOrderController : Controller
{
    private readonly IServiceOrderService _serviceOrderService;
    private readonly IVehicleService _vehicleService;
    private readonly IServiceTaskService _taskService;
    private readonly IWorkRateService _workRateService;
    private readonly IPartService _partService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public ServiceOrderController(
        IServiceOrderService serviceOrderService,
        IVehicleService vehicleService,
        IServiceTaskService taskService,
        IWorkRateService workRateService,
        IPartService partService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _serviceOrderService = serviceOrderService;
        _vehicleService = vehicleService;
        _taskService = taskService;
        _workRateService = workRateService;
        _partService = partService;
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
        => View(await _serviceOrderService.GetAllServiceOrdersAsync());

    [HttpGet]
    public async Task<IActionResult> Create()
        => View(new CreateServiceOrderViewModel
        {
            Vehicles = await _vehicleService.GetAllVehiclesAsync()
        });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateServiceOrderViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Vehicles = await _vehicleService.GetAllVehiclesAsync();
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var id = await _serviceOrderService.CreateOrderAsync(model, userId);
        TempData["SuccessMessage"] = "Zlecenie zostało utworzone.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var vm = await _serviceOrderService.GetOrderDetailsAsync(id);
        if (vm is null) return NotFound();

        await PopulateDropdownsAsync();

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTask(ServiceTaskFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = string.Join(" | ",
                ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Details), new { id = vm.ServiceOrderId });
        }

        await _taskService.AddServiceTaskAsync(vm);

        TempData["Success"] = "Dodano czynność.";
        return RedirectToAction(nameof(Details), new { id = vm.ServiceOrderId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPart(UsedPartFormViewModel vm)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        if (!ModelState.IsValid)
        {
            var orderId = await _taskService.GetTaskServiceOrderServiceIdAsync(vm.ServiceTaskId);
            return await Details(orderId);
        }

        await _taskService.AddUsedPartAsync(vm, userId);
        var oId = await _taskService.GetTaskServiceOrderServiceIdAsync(vm.ServiceTaskId);
        return RedirectToAction(nameof(Details), new { id = oId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, string newStatus)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var ok = await _serviceOrderService.ChangeStatusAsync(id, newStatus, userId);

        TempData[ok ? "Success" : "Error"] = ok ? "Status zaktualizowany." : "Brak uprawnień.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int serviceOrderId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            TempData["Error"] = "Komentarz nie może być pusty.";
            return RedirectToAction(nameof(Details), new { id = serviceOrderId });
        }

        var comment = new OrderComment
        {
            ServiceOrderId = serviceOrderId,
            AuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        _context.OrderComments.Add(comment);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Komentarz dodany.";
        return RedirectToAction(nameof(Details), new { id = serviceOrderId });
    }

    private async Task PopulateDropdownsAsync()
    {
        ViewBag.WorkRates = await _workRateService.GetSelectWorkRatesAsync();
        ViewBag.Parts = await _partService.GetActivePartsSelectAsync();
        ViewBag.Mechanics = new SelectList(
            await _userManager.GetUsersInRoleAsync(Roles.Mechanic),
            "Id", "UserName");
        ViewBag.Statuses = new SelectList(OrderStatuses.AllStatuses);
    }
}
