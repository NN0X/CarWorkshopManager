using System.Security.Claims;
using CarWorkshopManager.Constants;
using CarWorkshopManager.Documents;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.ServiceOrder;
using CarWorkshopManager.ViewModels.ServiceTasks;
using CarWorkshopManager.ViewModels.UsedPart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;

namespace CarWorkshopManager.Controllers
{
    [Authorize(Roles = $"{Roles.Receptionist},{Roles.Admin},{Roles.Mechanic}")]
    public class ServiceOrderController : Controller
    {
        private readonly IServiceOrderService _orderService;
        private readonly IOrderCommentService _commentService;
        private readonly IVehicleService _vehicleService;
        private readonly IServiceTaskService _serviceTaskService;
        private readonly ILogger<ServiceOrderController> _logger;

        public ServiceOrderController(
            IServiceOrderService orderService,
            IOrderCommentService commentService,
            IVehicleService vehicleService,
            IServiceTaskService serviceTaskService,
            ILogger<ServiceOrderController> logger)
        {
            _orderService = orderService;
            _commentService = commentService;
            _vehicleService = vehicleService;
            _serviceTaskService = serviceTaskService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Displaying service orders list");
            var orders = await _orderService.GetAllServiceOrdersAsync();
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? vehicleId = null)
        {
            _logger.LogInformation("Displaying Create ServiceOrder page (vehicleId={VehicleId})", vehicleId);
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
            _logger.LogInformation("Create ServiceOrder attempt: {@Model}", model);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Create ServiceOrder model invalid");
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("Create ServiceOrder: unauthorized user");
                return Unauthorized();
            }

            var id = await _orderService.CreateOrderAsync(model, userId);
            _logger.LogInformation("ServiceOrder created successfully (OrderId={OrderId})", id);
            TempData["SuccessMessage"] = "Zlecenie zostało utworzone.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            _logger.LogInformation("Displaying details for ServiceOrder {OrderId}", id);
            var vm = await _orderService.GetOrderDetailsAsync(id);
            if (vm == null)
            {
                _logger.LogWarning("ServiceOrder not found (OrderId={OrderId})", id);
                return NotFound();
            }

            await _orderService.PopulateDetailsViewModelAsync(vm, User);
            var (laborNet, laborVat, partsNet, partsVat) = await _orderService.GetOrderTotalsAsync(id);

            ViewBag.LaborNet = laborNet;
            ViewBag.LaborVat = laborVat;
            ViewBag.PartsNet = partsNet;
            ViewBag.PartsVat = partsVat;
            ViewBag.TotalNet = laborNet + partsNet;
            ViewBag.TotalVat = laborVat + partsVat;

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string newStatus)
        {
            _logger.LogInformation("ChangeStatus attempt for Order {OrderId} to {Status}", id, newStatus);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("ChangeStatus: unauthorized user");
                return Unauthorized();
            }

            var ok = await _orderService.ChangeStatusAsync(id, newStatus, userId);
            _logger.LogInformation("ChangeStatus result for Order {OrderId}: {Success}", id, ok);
            TempData[ok ? "Success" : "Error"] = ok ? "Status zaktualizowany." : "Brak uprawnień.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int serviceOrderId, string content)
        {
            _logger.LogInformation("AddComment attempt for Order {OrderId}", serviceOrderId);
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("AddComment: empty content for Order {OrderId}", serviceOrderId);
                TempData["Error"] = "Komentarz nie może być pusty.";
                return RedirectToAction(nameof(Details), new { id = serviceOrderId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("AddComment: unauthorized user");
                return Unauthorized();
            }

            await _commentService.AddCommentAsync(serviceOrderId, userId, content);
            _logger.LogInformation("Comment added to Order {OrderId}", serviceOrderId);
            TempData["Success"] = "Komentarz dodany.";
            return RedirectToAction(nameof(Details), new { id = serviceOrderId });
        }

        [Authorize(Roles = $"{Roles.Receptionist},{Roles.Admin}")]
        [HttpGet]
        public async Task<IActionResult> RepairCostReport(DateTime? month, int? vehicleId)
        {
            _logger.LogInformation("Displaying RepairCostReport (month={Month}, vehicleId={VehicleId})", month, vehicleId);
            var vm = await _orderService.GetRepairCostReportAsync(month, vehicleId);
            return View(vm);
        }

        [Authorize(Roles = $"{Roles.Receptionist},{Roles.Admin}")]
        [HttpGet]
        public async Task<IActionResult> RepairCostReportPdf(DateTime? month, int? vehicleId)
        {
            _logger.LogInformation("Generating RepairCostReport PDF (month={Month}, vehicleId={VehicleId})", month, vehicleId);
            var vm = await _orderService.GetRepairCostReportAsync(month, vehicleId);
            var document = new RepairCostReportDocument(vm);
            byte[] pdfBytes = document.GeneratePdf();
            string fileName = month.HasValue
                ? $"Raport_{month:yyyy_MM}.pdf"
                : "Raport_All.pdf";

            _logger.LogInformation("RepairCostReport PDF generated (FileName={FileName})", fileName);
            return File(pdfBytes, "application/pdf", fileName);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpGet]
        public IActionResult MonthlyRepairSummary()
        {
            _logger.LogInformation("Displaying MonthlyRepairSummary criteria page");
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
            _logger.LogInformation("MonthlyRepairSummary report request for month {Month}", criteria.Month);
            if (criteria.Month == default)
            {
                _logger.LogWarning("MonthlyRepairSummary: invalid month");
                ModelState.AddModelError(nameof(criteria.Month), "Wybierz miesiąc.");
            }

            if (!ModelState.IsValid)
                return View(criteria);

            var report = await _orderService.GetMonthlyRepairSummaryAsync(criteria.Month);
            _logger.LogInformation("MonthlyRepairSummary report generated for month {Month}", criteria.Month);
            return View("MonthlyRepairSummary", report);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpGet]
        public async Task<IActionResult> MonthlyRepairSummaryPdf(DateTime month)
        {
            _logger.LogInformation("Generating MonthlyRepairSummary PDF for month {Month}", month);
            var report = await _orderService.GetMonthlyRepairSummaryAsync(month);
            var doc = new MonthlyRepairSummaryDocument(report);
            var pdf = doc.GeneratePdf();
            string fileName = $"Podsumowanie_{month:yyyy_MM}.pdf";
            _logger.LogInformation("MonthlyRepairSummary PDF generated (FileName={FileName})", fileName);
            return File(pdf, "application/pdf", fileName);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTask(ServiceTaskFormViewModel model)
        {
            _logger.LogInformation("AddTask attempt: {@Model}", model);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("AddTask: model invalid for ServiceOrder {OrderId}", model.ServiceOrderId);
                var vm = await _orderService.GetOrderDetailsAsync(model.ServiceOrderId);
                await _orderService.PopulateDetailsViewModelAsync(vm, User);
                ViewBag.LaborNet = vm.Tasks.Sum(t => t.LaborCost);
                ViewBag.PartsNet = vm.Tasks.Sum(t => t.PartsCost);
                ViewBag.TotalNet = ViewBag.LaborNet + ViewBag.PartsNet;
                return View("Details", vm);
            }

            await _serviceTaskService.AddServiceTaskAsync(model);
            _logger.LogInformation("ServiceTask added to Order {OrderId}", model.ServiceOrderId);
            TempData["Success"] = "Czynność dodana.";
            return RedirectToAction(nameof(Details), new { id = model.ServiceOrderId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPart(UsedPartFormViewModel model)
        {
            _logger.LogInformation("AddPart attempt: {@Model}", model);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("AddPart: model invalid for Task {TaskId}", model.ServiceTaskId);
                var soId = await _serviceTaskService.GetTaskServiceOrderServiceIdAsync(model.ServiceTaskId);
                var vm = await _orderService.GetOrderDetailsAsync(soId);
                await _orderService.PopulateDetailsViewModelAsync(vm, User);
                return View("Details", vm);
            }

            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _serviceTaskService.AddUsedPartAsync(model, uid!);
            _logger.LogInformation("UsedPart added to Task {TaskId}", model.ServiceTaskId);
            TempData["Success"] = "Część dodana.";
            var orderId = await _serviceTaskService.GetTaskServiceOrderServiceIdAsync(model.ServiceTaskId);
            return RedirectToAction(nameof(Details), new { id = orderId });
        }
    }
}
