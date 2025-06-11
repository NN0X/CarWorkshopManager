using System.Security.Claims;
using CarWorkshopManager.Constants;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.ServiceOrder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarWorkshopManager.Controllers;

[Authorize(Roles = $"{Roles.Receptionist},{Roles.Admin}")]
public class ServiceOrderController : Controller
{
    private readonly IServiceOrderService _serviceOrderService;
    private readonly IVehicleService _vehicleService;

    public ServiceOrderController(IServiceOrderService serviceOrderService, IVehicleService vehicleService)
    {
        _serviceOrderService = serviceOrderService;
        _vehicleService = vehicleService;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vehicles = await _vehicleService.GetAllVehiclesAsync();

        var vm = new CreateServiceOrderViewModel
        {
            Vehicles = vehicles
        };
        
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateServiceOrderViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Vehicles = await _vehicleService.GetAllVehiclesAsync();
            return View(model);
        }
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }      
        
        var orderId = await _serviceOrderService.CreateOrderAsync(model, userId);

        TempData["SuccessMessage"] = "Zlecenie zostało utworzone.";
        return RedirectToAction("Details", "ServiceOrder", new { id = orderId });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        return View(model: id);
    }
}