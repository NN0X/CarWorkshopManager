using CarWorkshopManager.Constants;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CarWorkshopManager.Controllers;

[Authorize(Roles = $"{Roles.Receptionist}, {Roles.Admin}")]
public class CustomerController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(ICustomerService customerService,
                              ILogger<CustomerController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Add()
    {
        _logger.LogInformation("Displaying Add Customer page");
        return View(new CreateCustomerViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(CreateCustomerViewModel model)
    {
        _logger.LogInformation("Add Customer attempt: FirstName={FirstName}, LastName={LastName}, Phone={PhoneNumber}",
                                model.FirstName, model.LastName, model.PhoneNumber);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Add Customer model invalid for FirstName={FirstName}, LastName={LastName}",
                                model.FirstName, model.LastName);
            return View(model);
        }

        await _customerService.AddCustomerAsync(model);

        _logger.LogInformation("Customer added successfully: FirstName={FirstName}, LastName={LastName}",
                                model.FirstName, model.LastName);

        TempData["Success"] = "Klient został dodany.";
        return RedirectToAction("Add");
    }

    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Displaying Customer index");
        var customers = await _customerService.GetAllCustomersAsync();
        return View(customers);
    }

    public async Task<IActionResult> Details(int id)
    {
        _logger.LogInformation("Displaying Customer details for Id={CustomerId}", id);

        var vm = await _customerService.GetCustomerDetailsAsync(id);
        if (vm == null)
        {
            _logger.LogWarning("Customer not found for Id={CustomerId}", id);
            return NotFound();
        }

        return View(vm);
    }
}
