using CarWorkshopManager.Constants;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.Part;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarWorkshopManager.Controllers;

[Authorize(Roles = Roles.Admin)]
public class PartController : Controller
{
    private readonly IPartService _partService;
    private readonly IVatRateService _vatRateService;

    public PartController(IPartService partService, IVatRateService vatRateService)
    {
        _partService = partService;
        _vatRateService = vatRateService;
    }
    
    public async Task<IActionResult> Index()
    {
        var parts = await _partService.GetAllPartsAsync();
        return View(parts);
    }
    
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.VatRates = await _vatRateService.GetSelectVatRatesListAsync();
        return View(new PartFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PartFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.VatRates = await _vatRateService.GetSelectVatRatesListAsync();
            return View(vm);
        }

        await _partService.CreatePartAsync(vm);
        TempData["Success"] = "Część została dodana.";
        return RedirectToAction(nameof(Index));
    }
    
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _partService.GetPartByIdAsync(id);
        if (vm is null) 
            return NotFound();

        ViewBag.VatRates = await _vatRateService.GetSelectVatRatesListAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PartFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.VatRates = await _vatRateService.GetSelectVatRatesListAsync();
            return View(vm);
        }

        await _partService.UpdatePartAsync(vm);
        TempData["Success"] = "Część została zaktualizowana.";
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _partService.SoftDeletePartAsync(id);
        TempData["Success"] = "Część oznaczona jako nieaktywna.";
        return RedirectToAction(nameof(Index));
    }
}
