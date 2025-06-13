using CarWorkshopManager.Constants;
using CarWorkshopManager.Models.Identity;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CarWorkshopManager.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AdminController : Controller
{
    private readonly IUserRegistrationService _registrationService;
    private readonly IEmailSender _emailSender;
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IUserRegistrationService registrationService,
        IEmailSender emailSender,
        IAdminService adminService,
        ILogger<AdminController> logger)
    {
        _registrationService = registrationService;
        _emailSender = emailSender;
        _adminService = adminService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult RegisterUser()
    {
        _logger.LogInformation("Displaying RegisterUser page");
        return View(new RegisterUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterUser(RegisterUserViewModel model)
    {
        _logger.LogInformation("RegisterUser attempt for {Email}", model.Email);
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var (result, user, token) = await _registrationService.RegisterUserAsync(
                model.FirstName, model.LastName, model.Email, model.PhoneNumber, model.Role);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                _logger.LogWarning("Registration failed for {Email}: {Errors}",
                    model.Email, string.Join(";", result.Errors.Select(e => e.Description)));
                return View(model);
            }

            var resetLink = Url.Action(
                "ResetPassword", "Account",
                new { token, email = user.Email },
                Request.Scheme);

            await _emailSender.SendEmailAsync(
                user.Email,
                "CarworkshopManager - ustaw swoje hasło!",
                $"<p>Cześć {user.FirstName},</p>...");

            _logger.LogInformation("User {User} registered and email sent", user.UserName);
            TempData["Success"] = $"Użytkownik {user.UserName} został utworzony, link wysłano na {user.Email}.";
            return RedirectToAction("RegisterUser");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during RegisterUser for {Email}", model.Email);
            throw;
        }
    }

    [HttpGet]
    public async Task<IActionResult> Users()
    {
        _logger.LogInformation("Displaying Users list");
        var users = await _adminService.GetAllUsersAsync();
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(string userId, string newRole)
    {
        _logger.LogInformation("ChangeRole attempt for {UserId} to {Role}", userId, newRole);
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(newRole))
        {
            TempData["Error"] = "Nieprawidłowe dane.";
            _logger.LogWarning("ChangeRole invalid data");
            return RedirectToAction(nameof(Users));
        }

        var success = await _adminService.ChangeUserRoleAsync(userId, newRole);
        _logger.LogInformation("ChangeRole result for {UserId}: {Success}", userId, success);
        TempData[success ? "Success" : "Error"] =
            success ? "Zmieniono rolę użytkownika." : "Nie udało się zmienić roli.";
        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        _logger.LogInformation("Displaying Edit page for {UserId}", id);
        var user = await _adminService.GetUserByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("Edit: user not found {UserId}", id);
            return NotFound();
        }

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ApplicationUser user)
    {
        _logger.LogInformation("Edit attempt for {UserId}", user.Id);
        if (!ModelState.IsValid)
            return View(user);

        var result = await _adminService.UpdateUserAsync(user);
        if (result.Succeeded)
        {
            _logger.LogInformation("Edit successful for {UserId}", user.Id);
            TempData["Success"] = "Dane użytkownika zostały zaktualizowane.";
            return RedirectToAction(nameof(Users));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
        _logger.LogWarning("Edit failed for {UserId}: {Errors}",
            user.Id, string.Join(";", result.Errors.Select(e => e.Description)));
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        _logger.LogInformation("Delete attempt for {UserId}", id);
        var result = await _adminService.DeleteUserAsync(id);
        _logger.LogInformation("Delete result for {UserId}: {Succeeded}", id, result.Succeeded);
        TempData[result.Succeeded ? "Success" : "Error"] =
            result.Succeeded ? "Użytkownik usunięty." : "Nie można usunąć użytkownika – jest powiązany z innymi danymi.";
        return RedirectToAction(nameof(Users));
    }
}
