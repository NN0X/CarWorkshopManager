using CarWorkshopManager.Constants;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.Part;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace CarWorkshopManager.Controllers;

[Authorize(Roles = Roles.Admin)]
public class PartController : Controller
{
    private readonly IPartService _partService;
    private readonly IVatRateService _vatRateService;
    private readonly ILogger<PartController> _logger;

    public PartController(
        IPartService partService,
        IVatRateService vatRateService,
        ILogger<PartController> logger)
    {
        _partService = partService;
        _vatRateService = vatRateService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Displaying Parts index");
        var parts = await _partService.GetAllPartsAsync();
        return View(parts);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        _logger.LogInformation("Displaying Create Part page");
        ViewBag.VatRates = await _vatRateService.GetSelectVatRatesListAsync();
        return View(new PartFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PartFormViewModel vm)
    {
        _logger.LogInformation("Create Part attempt: {@Model}", vm);
        if (!ModelState.IsValid)
        {
            ViewBag.VatRates = await _vatRateService.GetSelectVatRatesListAsync();
            return View(vm);
        }

        await _partService.CreatePartAsync(vm);
        _logger.LogInformation("Part created: {Name}", vm.Name);
        TempData["Success"] = "Część została dodana.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        _logger.LogInformation("Displaying Edit Part page for {Id}", id);
        var vm = await _partService.GetPartByIdAsync(id);
        if (vm is null)
        {
            _logger.LogWarning("Part not found {Id}", id);
            return NotFound();
        }

        ViewBag.VatRates = await _vatRateService.GetSelectVatRatesListAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PartFormViewModel vm)
    {
        _logger.LogInformation("Edit Part attempt: {@Model}", vm);
        if (!ModelState.IsValid)
        {
            ViewBag.VatRates = await _vatRateService.GetSelectVatRatesListAsync();
            return View(vm);
        }

        await _partService.UpdatePartAsync(vm);
        _logger.LogInformation("Part updated: {Id}", vm.Id);
        TempData["Success"] = "Część została zaktualizowana.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Delete Part attempt for {Id}", id);
        await _partService.SoftDeletePartAsync(id);
        _logger.LogInformation("Part soft-deleted: {Id}", id);
        TempData["Success"] = "Część oznaczona jako nieaktywna.";
        return RedirectToAction(nameof(Index));
    }
}
