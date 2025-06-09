using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarWorkshopManager.Controllers;

public class DashboardController : Controller
{
    [Authorize]
    public IActionResult Index()
    {
        return View();
    }
}