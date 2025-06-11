using CarWorkshopManager.Models.Identity;
using CarWorkshopManager.ViewModels.Admin;
using Microsoft.AspNetCore.Identity;

namespace CarWorkshopManager.Services.Interfaces;

public interface IAdminService
{
    Task<List<UserListItemViewModel>> GetAllUsersAsync();
    Task<bool> ChangeUserRoleAsync(string userId, string newRole);
    Task<IdentityResult> DeleteUserAsync(string userId);
    Task<ApplicationUser?> GetUserByIdAsync(string userId); 
    Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
}