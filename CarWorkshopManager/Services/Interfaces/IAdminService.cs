using CarWorkshopManager.ViewModels.Admin;

namespace CarWorkshopManager.Services.Interfaces;

public interface IAdminService
{
    Task<List<UserListItemViewModel>> GetAllUsersAsync();
    Task<bool> ChangeUserRoleAsync(string userId, string newRole);
}