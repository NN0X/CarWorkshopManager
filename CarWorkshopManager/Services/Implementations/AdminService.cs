using CarWorkshopManager.Models.Identity;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarWorkshopManager.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminService> _logger;

        public AdminService(UserManager<ApplicationUser> userManager,
                            ILogger<AdminService> logger)
        {
            _userManager = userManager;
            _logger      = logger;
        }

        public async Task<List<UserListItemViewModel>> GetAllUsersAsync()
        {
            _logger.LogInformation("Retrieving all users");
            var users = await _userManager.Users.ToListAsync();
            var result = new List<UserListItemViewModel>();

            foreach (var user in users)
            {
                var role = (await _userManager.GetRolesAsync(user))
                            .FirstOrDefault() ?? "Brak";
                result.Add(new UserListItemViewModel
                {
                    Id          = user.Id,
                    Username    = user.UserName ?? "",
                    FullName    = $"{user.FirstName} {user.LastName}",
                    Email       = user.Email ?? "",
                    PhoneNumber = user.PhoneNumber ?? "",
                    Role        = role
                });
            }

            _logger.LogInformation("Retrieved {Count} users", result.Count);
            return result;
        }

        public async Task<bool> ChangeUserRoleAsync(string userId, string newRole)
        {
            _logger.LogInformation("Changing role for user {UserId} to {Role}", userId, newRole);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return false;
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    _logger.LogError("Failed to remove existing roles from {UserId}: {Errors}",
                                     userId, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                    return false;
                }
            }

            var addResult = await _userManager.AddToRoleAsync(user, newRole);
            if (!addResult.Succeeded)
            {
                _logger.LogError("Failed to add role {Role} to {UserId}: {Errors}",
                                 newRole, userId, string.Join(", ", addResult.Errors.Select(e => e.Description)));
            }
            else
            {
                _logger.LogInformation("Role changed successfully for {UserId}", userId);
            }

            return addResult.Succeeded;
        }

        public async Task<IdentityResult> DeleteUserAsync(string userId)
        {
            _logger.LogInformation("Deleting user {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return IdentityResult.Failed(new IdentityError { Description = "Użytkownik nie istnieje." });
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                _logger.LogError("Failed to delete {UserId}: {Errors}",
                                 userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            else
                _logger.LogInformation("Deleted user {UserId}", userId);

            return result;
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            _logger.LogInformation("Getting user by Id: {UserId}", userId);
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<IdentityResult> UpdateUserAsync(ApplicationUser updatedUser)
        {
            _logger.LogInformation("Updating user {UserId}", updatedUser.Id);

            var existing = await _userManager.FindByIdAsync(updatedUser.Id);
            if (existing == null)
            {
                _logger.LogWarning("User not found: {UserId}", updatedUser.Id);
                return IdentityResult.Failed(new IdentityError { Description = "Użytkownik nie istnieje." });
            }

            existing.FirstName       = updatedUser.FirstName;
            existing.LastName        = updatedUser.LastName;
            existing.Email           = updatedUser.Email;
            existing.NormalizedEmail = _userManager.NormalizeEmail(updatedUser.Email);
            existing.PhoneNumber     = updatedUser.PhoneNumber;

            var result = await _userManager.UpdateAsync(existing);
            if (!result.Succeeded)
                _logger.LogError("Failed to update {UserId}: {Errors}",
                                 updatedUser.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
            else
                _logger.LogInformation("Updated user {UserId}", updatedUser.Id);

            return result;
        }
    }
}
