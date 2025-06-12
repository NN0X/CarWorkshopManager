using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CarWorkshopManager.Controllers;

public class DashboardController : Controller
{
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ILogger<DashboardController> logger)
    {
        _logger = logger;
    }

    [Authorize]
    public IActionResult Index()
    {
        _logger.LogInformation("Displaying Dashboard");
        return View();
    }
}
