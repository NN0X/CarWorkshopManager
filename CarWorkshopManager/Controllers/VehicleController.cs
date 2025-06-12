using CarWorkshopManager.Constants;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.Vehicle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = $"{Roles.Receptionist},{Roles.Admin}")]
public class VehicleController : Controller
{
    private readonly IVehicleService _vehicleService;

    public VehicleController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet]
    public IActionResult Add(int customerId)
    {
        return View(new AddVehicleViewModel { CustomerId = customerId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(AddVehicleViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        await _vehicleService.AddVehicleAsync(vm);
        TempData["Success"] = "Pojazd dodany.";

        return RedirectToAction("Details", "Customer", new { id = vm.CustomerId });
    }

    public async Task<IActionResult> Index()
    {
        return View(await _vehicleService.GetAllVehiclesAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _vehicleService.GetEditVehicleAsync(id);
        return vm == null ? NotFound() : View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(VehicleEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        await _vehicleService.UpdateVehicleAsync(vm);
        TempData["Success"] = "Zaktualizowano pojazd.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPhoto(int id, IFormFile image)
    {
        await _vehicleService.UploadVehiclePhotoAsync(id, image);
        TempData["Success"] = "Zdjęcie zapisane.";
        return RedirectToAction(nameof(Index));
    }
}
