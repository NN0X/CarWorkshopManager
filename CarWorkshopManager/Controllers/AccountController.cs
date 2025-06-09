using CarWorkshopManager.Models.Identity;
using CarWorkshopManager.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CarWorkshopManager.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel lvm)
    {
        if (!ModelState.IsValid)
            return View(lvm);
        
        var result = await _signInManager.PasswordSignInAsync(lvm.Username, lvm.Password, lvm.RememberMe, false);
        
        if (result.Succeeded)
            return RedirectToAction("Index", "Home");
        
        ModelState.AddModelError(string.Empty, "Niepoprawna nazwa użytkownika lub hasło.");
        return View(lvm);
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpGet]
    public IActionResult ResetPassword(string token, string email)
    {
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
        if (!ModelState.IsValid)
            return View(vm);

        var user = await _userManager.FindByEmailAsync(vm.Email);
        if (user == null)
            return RedirectToAction(nameof(ResetPasswordConfirmation));

        var result = await _userManager.ResetPasswordAsync(user, vm.Token, vm.Password);
        if (result.Succeeded)
            return RedirectToAction(nameof(ResetPasswordConfirmation));

        foreach (var e in result.Errors)
            ModelState.AddModelError(string.Empty, e.Description);
        return View(vm);
    }
    
    [HttpGet]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }
}