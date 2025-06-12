using QuestPDF.Fluent;
using System.Security.Claims;
using CarWorkshopManager.Constants;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.ServiceOrder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarWorkshopManager.Documents;
using CarWorkshopManager.ViewModels.ServiceTasks;
using CarWorkshopManager.ViewModels.UsedPart;

namespace CarWorkshopManager.Controllers
{
    [Authorize(Roles = $"{Roles.Receptionist},{Roles.Admin},{Roles.Mechanic}")]
    public class ServiceOrderController : Controller
    {
        private readonly IServiceOrderService _orderService;
        private readonly IOrderCommentService _commentService;
        private readonly IVehicleService _vehicleService;
        private readonly IServiceTaskService _serviceTaskService;

        public ServiceOrderController(IServiceOrderService orderService, IOrderCommentService commentService,
            IVehicleService vehicleService, IServiceTaskService serviceTaskService)
        {
            _orderService = orderService;
            _commentService = commentService;
            _vehicleService = vehicleService;
            _serviceTaskService = serviceTaskService;
        }

        public async Task<IActionResult> Index()
            => View(await _orderService.GetAllServiceOrdersAsync());

        [HttpGet]
        public async Task<IActionResult> Create(int? vehicleId = null)
        {
            var vm = new CreateServiceOrderViewModel
            {
                VehicleId = vehicleId ?? 0,
                Vehicles = await _vehicleService.GetAllVehiclesAsync()
            };

            ViewBag.IsVehiclePreselected = vehicleId.HasValue;
            return View(vm);
        }

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
            if (vm == null) 
                return NotFound();

            await _orderService.PopulateDetailsViewModelAsync(vm, User);

       
            var (laborNet, laborVat, partsNet, partsVat) = await _orderService.GetOrderTotalsAsync(id);

            ViewBag.LaborNet =  laborNet;
            ViewBag.LaborVat =  laborVat;
            ViewBag.PartsNet =  partsNet;
            ViewBag.PartsVat =  partsVat;
            ViewBag.TotalNet = laborNet + partsNet;
            ViewBag.TotalVat = laborVat + partsVat;

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string newStatus)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var ok = await _orderService.ChangeStatusAsync(id, newStatus, userId);
            TempData[ok ? "Success" : "Error"] = ok ? "Status zaktualizowany." : "Brak uprawnień.";
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
            if (userId == null) 
                return Unauthorized();

            await _commentService.AddCommentAsync(serviceOrderId, userId, content);
            TempData["Success"] = "Komentarz dodany.";
            return RedirectToAction(nameof(Details), new { id = serviceOrderId });
        }

        [Authorize(Roles = $"{Roles.Receptionist},{Roles.Admin}")]
        [HttpGet]
        public async Task<IActionResult> RepairCostReport(DateTime? month, int? vehicleId)
        {
            var vm = await _orderService.GetRepairCostReportAsync(month, vehicleId);
            return View(vm);
        }

        [Authorize(Roles = $"{Roles.Receptionist},{Roles.Admin}")]
        [HttpGet]
        public async Task<IActionResult> RepairCostReportPdf(DateTime? month, int? vehicleId)
        {
            RepairCostReportViewModel vm = await _orderService.GetRepairCostReportAsync(month, vehicleId);
            var document = new RepairCostReportDocument(vm);
            byte[] pdfBytes = document.GeneratePdf();
            string fileName = month.HasValue
                ? $"Raport_{month:yyyy_MM}.pdf"
                : "Raport_All.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpGet]
        public IActionResult MonthlyRepairSummary()
        {
            var vm = new MonthlyRepairSummaryReportViewModel
            {
                Month = DateTime.Today
            };
            return View(vm);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost]
        public async Task<IActionResult> MonthlyRepairSummary(MonthlyRepairSummaryReportViewModel criteria)
        {
            if (criteria.Month == default)
                ModelState.AddModelError(nameof(criteria.Month), "Wybierz miesiąc.");

            if (!ModelState.IsValid)
                return View(criteria);

            var report = await _orderService.GetMonthlyRepairSummaryAsync(criteria.Month);
            return View("MonthlyRepairSummary", report);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpGet]
        public async Task<IActionResult> MonthlyRepairSummaryPdf(DateTime month)
        {
            var report = await _orderService.GetMonthlyRepairSummaryAsync(month);
            var doc = new MonthlyRepairSummaryDocument(report);
            var pdf = doc.GeneratePdf();
            return File(pdf, "application/pdf", $"Podsumowanie_{month:yyyy_MM}.pdf");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTask(ServiceTaskFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orderService.GetOrderDetailsAsync(model.ServiceOrderId);
                await _orderService.PopulateDetailsViewModelAsync(vm, User);
                return View("Details", vm);
            }

            await _serviceTaskService.AddServiceTaskAsync(model);
            TempData["Success"] = "Czynność dodana.";
            return RedirectToAction(nameof(Details), new { id = model.ServiceOrderId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPart(UsedPartFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var soId = await _serviceTaskService.GetTaskServiceOrderServiceIdAsync(model.ServiceTaskId);
                var vm = await _orderService.GetOrderDetailsAsync(soId);
                await _orderService.PopulateDetailsViewModelAsync(vm, User);
                return View("Details", vm);
            }

            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _serviceTaskService.AddUsedPartAsync(model, uid!);

            TempData["Success"] = "Część dodana.";
            var orderId = await _serviceTaskService.GetTaskServiceOrderServiceIdAsync(model.ServiceTaskId);
            return RedirectToAction(nameof(Details), new { id = orderId });
        }
    }
}
