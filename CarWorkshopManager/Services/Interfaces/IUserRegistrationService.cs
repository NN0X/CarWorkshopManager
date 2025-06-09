using CarWorkshopManager.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace CarWorkshopManager.Services.Interfaces;

public interface IUserRegistrationService
{
    Task<(IdentityResult result, ApplicationUser user, string token)>
        RegisterUserAsync(string firstName, string lastName, string email, string? phoneNumber, string role);
}