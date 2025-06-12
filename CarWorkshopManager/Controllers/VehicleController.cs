using CarWorkshopManager.Constants;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.Vehicle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CarWorkshopManager.Controllers;

[Authorize(Roles = $"{Roles.Receptionist},{Roles.Admin}")]
public class VehicleController : Controller
{
    private readonly IVehicleService _vehicleService;
    private readonly ILogger<VehicleController> _logger;

    public VehicleController(
        IVehicleService vehicleService,
        ILogger<VehicleController> logger)
    {
        _vehicleService = vehicleService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Add(int customerId)
    {
        _logger.LogInformation("Displaying Add Vehicle page for Customer {CustomerId}", customerId);
        return View(new AddVehicleViewModel { CustomerId = customerId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(AddVehicleViewModel vm)
    {
        _logger.LogInformation("Add Vehicle attempt: {@Model}", vm);
        if (!ModelState.IsValid)
            return View(vm);

        await _vehicleService.AddVehicleAsync(vm);
        _logger.LogInformation("Vehicle added for Customer {CustomerId}", vm.CustomerId);
        TempData["Success"] = "Pojazd dodany.";
        return RedirectToAction("Details", "Customer", new { id = vm.CustomerId });
    }

    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Displaying Vehicle index");
        var list = await _vehicleService.GetAllVehiclesAsync();
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        _logger.LogInformation("Displaying Edit Vehicle page for {Id}", id);
        var vm = await _vehicleService.GetEditVehicleAsync(id);
        return vm == null ? NotFound() : View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(VehicleEditViewModel vm)
    {
        _logger.LogInformation("Edit Vehicle attempt: {@Model}", vm);
        if (!ModelState.IsValid)
            return View(vm);

        await _vehicleService.UpdateVehicleAsync(vm);
        _logger.LogInformation("Vehicle updated: {Id}", vm.Id);
        TempData["Success"] = "Zaktualizowano pojazd.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPhoto(int id, IFormFile image)
    {
        _logger.LogInformation("UploadPhoto attempt for Vehicle {Id}", id);
        await _vehicleService.UploadVehiclePhotoAsync(id, image);
        _logger.LogInformation("Photo uploaded for Vehicle {Id}", id);
        TempData["Success"] = "Zdjęcie zapisane.";
        return RedirectToAction(nameof(Index));
    }
}
