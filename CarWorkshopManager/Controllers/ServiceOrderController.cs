using System.Security.Claims;
using System.Threading.Tasks;
using CarWorkshopManager.Constants;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.ServiceOrder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarWorkshopManager.Controllers
{
    [Authorize(Roles = $"{Roles.Receptionist},{Roles.Admin},{Roles.Mechanic}")]
    public class ServiceOrderController : Controller
    {
        private readonly IServiceOrderService _orderService;
        private readonly IOrderCommentService _commentService;

        public ServiceOrderController(
            IServiceOrderService orderService,
            IOrderCommentService commentService)
        {
            _orderService = orderService;
            _commentService = commentService;
        }

        public async Task<IActionResult> Index()
            => View(await _orderService.GetAllServiceOrdersAsync());

        [HttpGet]
        public IActionResult Create() => View(new CreateServiceOrderViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateServiceOrderViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var id = await _orderService.CreateOrderAsync(model, userId);
            TempData["SuccessMessage"] = "Zlecenie zostało utworzone.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var vm = await _orderService.GetOrderDetailsAsync(id);
            if (vm == null) return NotFound();

            await _orderService.PopulateDetailsViewModelAsync(vm, User);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string newStatus)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var ok = await _orderService.ChangeStatusAsync(id, newStatus, userId);
            TempData[ok ? "Success" : "Error"] =
                ok ? "Status zaktualizowany." : "Brak uprawnień.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int serviceOrderId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Komentarz nie może być pusty.";
                return RedirectToAction(nameof(Details), new { id = serviceOrderId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            await _commentService.AddCommentAsync(serviceOrderId, userId, content);
            TempData["Success"] = "Komentarz dodany.";
            return RedirectToAction(nameof(Details), new { id = serviceOrderId });
        }
    }
}
