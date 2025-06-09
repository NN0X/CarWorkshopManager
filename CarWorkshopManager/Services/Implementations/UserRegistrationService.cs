using CarWorkshopManager.Models.Identity;
using CarWorkshopManager.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CarWorkshopManager.Services.Implementations;

public class UserRegistrationService : IUserRegistrationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUsernameGeneratorService _usernameGenerator;

    public UserRegistrationService(UserManager<ApplicationUser> userManager,
        IUsernameGeneratorService usernameGenerator)
    {
        _userManager = userManager;
        _usernameGenerator = usernameGenerator;
    }

    public async Task<(IdentityResult result, ApplicationUser user, string token)> 
        RegisterUserAsync(string firstName, string lastName, string email, string? phoneNumber, string role)
    {
        var username = await _usernameGenerator.GenerateUsernameAsync(firstName, lastName);

        var user = new ApplicationUser
        {
            UserName = username,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            EmailConfirmed = true
        };
        
        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
            return (createResult, null!, null!);
        
        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
            return (roleResult, user, null!);

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        return (IdentityResult.Success, user, token);
    }
}