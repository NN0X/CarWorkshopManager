using System.Security.Claims;
using CarWorkshopManager.Constants;
using CarWorkshopManager.Models.Identity;
using CarWorkshopManager.Services.Implementations;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.ServiceOrder;
using CarWorkshopManager.ViewModels.ServiceTask;
using CarWorkshopManager.ViewModels.UsedPart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;

namespace CarWorkshopManager.Controllers;

[Authorize(Roles = $"{Roles.Receptionist},{Roles.Admin},{Roles.Mechanic}")]
public class ServiceOrderController : Controller
{
    private readonly IServiceOrderService _serviceOrderService;
    private readonly IVehicleService      _vehicleService;

    public ServiceOrderController(IServiceOrderService serviceOrderService,
                                  IVehicleService vehicleService)
    {
        _serviceOrderService = serviceOrderService;
        _vehicleService      = vehicleService;
    }

    /* ───── LISTA ───── */
    public async Task<IActionResult> Index()
        => View(await _serviceOrderService.GetAllServiceOrdersAsync());

    /* ───── UTWORZ ───── */
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

    /* ───── SZCZEGÓŁY ───── */
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var vm = await _serviceOrderService.GetOrderDetailsAsync(id);
        if (vm is null) return NotFound();

        /* dropdowny */
        ViewBag.WorkRates = await HttpContext.RequestServices
            .GetRequiredService<IWorkRateService>().GetSelectWorkRatesAsync();
        ViewBag.Parts = await HttpContext.RequestServices
            .GetRequiredService<IPartService>().GetActivePartsSelectAsync();
        ViewBag.Mechanics = new SelectList(
            await HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>()
                .GetUsersInRoleAsync(Roles.Mechanic),
            "Id", "UserName");
        ViewBag.Statuses = new SelectList(OrderStatuses.AllStatuses);

        return View(vm);
    }
    
    // ServiceOrderController.cs
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

        var taskService = HttpContext.RequestServices.GetRequiredService<IServiceTaskService>();

        await taskService.AddServiceTaskAsync(vm);

        TempData["Success"] = "Dodano czynność.";
        return RedirectToAction(nameof(Details), new { id = vm.ServiceOrderId });
    }
    
    /* --- dodaj część --- */
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPart(UsedPartFormViewModel vm)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var taskService = HttpContext.RequestServices.GetRequiredService<IServiceTaskService>();
        if (!ModelState.IsValid)
        {
            var orderId = await taskService.GetTaskServiceOrderServiceIdAsync(vm.ServiceTaskId);
            return await Details(orderId);
        }

        await taskService.AddUsedPartAsync(vm, userId);
        var oId = await taskService.GetTaskServiceOrderServiceIdAsync(vm.ServiceTaskId);
        return RedirectToAction(nameof(Details), new { id = oId });
    }

    /* --- zmiana statusu --- */
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, string newStatus)
    {
        var ok = await _serviceOrderService.ChangeStatusAsync(
            id, newStatus, User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        TempData[ok ? "Success" : "Error"] = ok ? "Status zaktualizowany." : "Brak uprawnień.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
