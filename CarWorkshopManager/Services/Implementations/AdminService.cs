using CarWorkshopManager.Models.Identity;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CarWorkshopManager.Services.Implementations;

public class AdminService : IAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<UserListItemViewModel>> GetAllUsersAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        
        var result = new List<UserListItemViewModel>();

        foreach (var user in users)
        {
            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Brak";
            
            result.Add(new UserListItemViewModel
            {
                Id = user.Id,
                Username = user.UserName ?? "",
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Role = role
            });
        }

        return result;
    }

    public async Task<bool> ChangeUserRoleAsync(string userId, string newRole)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return false;
        
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                return false;
        }
        
        var result = await _userManager.AddToRoleAsync(user, newRole);
        return result.Succeeded;
    }

    public async Task<IdentityResult> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "Uzytkownik nie istnieje. " });
        }
        
        return await _userManager.DeleteAsync(user);
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<IdentityResult> UpdateUserAsync(ApplicationUser updatedUser)
    {
        var existingUser = await _userManager.FindByIdAsync(updatedUser.Id);
        if (existingUser == null)
            return IdentityResult.Failed(new IdentityError { Description = "Użytkownik nie istnieje." });
        
        existingUser.FirstName = updatedUser.FirstName;
        existingUser.LastName = updatedUser.LastName;
        existingUser.Email = updatedUser.Email;
        existingUser.NormalizedEmail = _userManager.NormalizeEmail(updatedUser.Email);
        existingUser.PhoneNumber = updatedUser.PhoneNumber;

        return await _userManager.UpdateAsync(existingUser);
    }
}
