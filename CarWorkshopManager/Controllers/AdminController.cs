using CarWorkshopManager.Constants;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace CarWorkshopManager.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AdminController : Controller
{
    private readonly IUserRegistrationService _registrationService;
    private readonly IEmailSender _emailSender;
    private readonly IAdminService _adminService;

    public AdminController(IUserRegistrationService registrationService, IEmailSender emailSender, IAdminService adminService)
    {
        _registrationService = registrationService;
        _emailSender = emailSender;
        _adminService = adminService;
    }

    [HttpGet]
    public IActionResult RegisterUser()
    {
        return View(new RegisterUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterUser(RegisterUserViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var (result, user, token) = await _registrationService.RegisterUserAsync(
            model.FirstName, model.LastName, model.Email, model.PhoneNumber, model.Role);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }
        
        var resetLink = Url.Action("ResetPassword", "Account", new { token, email = user.Email }, Request.Scheme);

        var subject = "CarworkshopManager - ustaw swoje hasło!";
        var htmlMessage = $@"
                <p>Cześć {user.FirstName},</p>
                <p>Twoje konto zostało utworzone.</p>
                <p>Login: <strong>{user.UserName}</strong></p>
                <p>Kliknij poniższy link, by ustawić swoje hasło:</p>
                <p><a href=""{resetLink}"">Ustaw hasło</a></p>
                <br/>
                <p>Pozdrawiamy,<br/>Zespół CarWorkshopManager</p>";
        
        await _emailSender.SendEmailAsync(user.Email, subject, htmlMessage);
        
        TempData["Success"] = $"Użytkownik {user.UserName} został utworzony, link wysłano na {user.Email}.";
        return RedirectToAction("RegisterUser");
    }

    [HttpGet]
    public async Task<IActionResult> Users()
    {
        var users = await _adminService.GetAllUsersAsync();
        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> EditRoles()
    {
        var users = await _adminService.GetAllUsersAsync();
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(string userId, string newRole)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(newRole))
        {
            TempData["Error"] = "Nieprawidłowe dane.";
            return RedirectToAction(nameof(Users));
        }

        var success = await _adminService.ChangeUserRoleAsync(userId, newRole);
        TempData[success ? "Success" : "Error"] = success ? "Zmieniono rolę użytkownika." : "Nie udało się zmienić roli.";

        return RedirectToAction(nameof(EditRoles));
    }
}