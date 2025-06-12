using CarWorkshopManager.Models.Identity;
using CarWorkshopManager.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CarWorkshopManager.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        _logger.LogInformation("User {User} logging out", User.Identity?.Name);
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel lvm)
    {
        _logger.LogInformation("Login attempt for {Username}", lvm.Username);
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Login model invalid for {Username}", lvm.Username);
            return View(lvm);
        }

        try
        {
            var result = await _signInManager.PasswordSignInAsync(
                lvm.Username, lvm.Password, lvm.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("Login successful for {Username}", lvm.Username);
                return RedirectToAction("Index", "Home");
            }

            _logger.LogWarning("Login failed for {Username}", lvm.Username);
            ModelState.AddModelError(string.Empty, "Niepoprawna nazwa użytkownika lub hasło.");
            return View(lvm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during Login for {Username}", lvm.Username);
            throw;
        }
    }

    [HttpGet]
    public IActionResult Login()
    {
        _logger.LogInformation("Displaying Login page");
        return View(new LoginViewModel());
    }

    [HttpGet]
    public IActionResult ResetPassword(string token, string email)
    {
        _logger.LogInformation("Displaying ResetPassword page for email {Email}", email);
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            return BadRequest();

        var vm = new ResetPasswordViewModel
        {
            Token = token,
            Email = email
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
    {
        _logger.LogInformation("ResetPassword attempt for {Email}", vm.Email);
        if (!ModelState.IsValid)
            return View(vm);

        try
        {
            var user = await _userManager.FindByEmailAsync(vm.Email);
            if (user == null)
            {
                _logger.LogWarning("ResetPassword: user not found for {Email}", vm.Email);
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(user, vm.Token, vm.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("Password reset successful for {Email}", vm.Email);
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            _logger.LogWarning("Password reset failed for {Email}: {Errors}",
                vm.Email, string.Join(";", result.Errors.Select(e => e.Description)));
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during ResetPassword for {Email}", vm.Email);
            throw;
        }
    }

    [HttpGet]
    public IActionResult ResetPasswordConfirmation()
    {
        _logger.LogInformation("Displaying ResetPasswordConfirmation page");
        return View();
    }
}
