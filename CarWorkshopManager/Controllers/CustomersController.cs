using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarWorkshopManager.Data;
using CarWorkshopManager.Models.Domain;

namespace CarWorkshopManager.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CustomersController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _db.Customers
                                     .OrderBy(c => c.LastName)
                                     .ToListAsync();
            return View(customers);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,PhoneNumber,Email")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.CreatedAt = DateTime.UtcNow;
                _db.Add(customer);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }
    }
}
