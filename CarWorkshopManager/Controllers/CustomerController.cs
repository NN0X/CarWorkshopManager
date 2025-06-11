using CarWorkshopManager.Constants;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarWorkshopManager.Controllers;

[Authorize(Roles = $"{Roles.Receptionist}, {Roles.Admin}")]
public class CustomerController : Controller
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public IActionResult Add()
    {
        return View(new CreateCustomerViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(CreateCustomerViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        
        await _customerService.AddCustomerAsync(model);
        TempData["Success"] = "Klient został dodany.";
        return RedirectToAction("Add");
    }

    public async Task<IActionResult> Index()
    {
        var customers = await _customerService.GetAllCustomersAsync();
        return View(customers);
    }
    
    public async Task<IActionResult> Details(int id)
    {
        var vm = await _customerService.GetCustomerDetailsAsync(id);
        if (vm == null) 
            return NotFound();

        return View(vm);
    } 
}