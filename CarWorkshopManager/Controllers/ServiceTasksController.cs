using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarWorkshopManager.Data;
using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.Models.Identity;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.ServiceTasks;

namespace CarWorkshopManager.Controllers
{
    [Authorize]
    public class ServiceTasksController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ITaskCommentService _commentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ServiceTasksController(
            ApplicationDbContext db,
            ITaskCommentService commentService,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _commentService = commentService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var task = await _db.ServiceTasks
                                .Include(st => st.ServiceOrder) 
                                .FirstOrDefaultAsync(st => st.Id == id);
            if (task == null)
                return NotFound();

            var comments = await _commentService.GetCommentsForTaskAsync(id);

            var vm = new ServiceTaskDetailsViewModel
            {
                ServiceTask = task,
                Comments = comments.Select(c => new TaskCommentViewModel
                {
                    Id = c.Id,
                    UserName = c.Author.UserName,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt
                }).ToList(),
                NewComment = new NewTaskCommentViewModel
                {
                    ServiceTaskId = id
                }
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(NewTaskCommentViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var task = await _db.ServiceTasks
                                    .Include(st => st.ServiceOrder)
                                    .FirstOrDefaultAsync(st => st.Id == vm.ServiceTaskId);
                if (task == null)
                    return NotFound();

                var comments = await _commentService.GetCommentsForTaskAsync(vm.ServiceTaskId);
                var vmDetails = new ServiceTaskDetailsViewModel
                {
                    ServiceTask = task,
                    Comments = comments.Select(c => new TaskCommentViewModel
                    {
                        Id = c.Id,
                        UserName = c.Author.UserName,
                        Content = c.Content,
                        CreatedAt = c.CreatedAt
                    }).ToList(),
                    NewComment = vm
                };
                return View("Details", vmDetails);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            await _commentService.AddCommentAsync(vm.ServiceTaskId, user.Id, vm.Content);
            return RedirectToAction(nameof(Details), new { id = vm.ServiceTaskId });
        }
    }
}
