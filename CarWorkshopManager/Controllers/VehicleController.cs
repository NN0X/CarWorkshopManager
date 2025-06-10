using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.Vehicle;
using Microsoft.AspNetCore.Mvc;

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
}