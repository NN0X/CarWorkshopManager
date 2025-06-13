using CarWorkshopManager.Models.Identity;
using CarWorkshopManager.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CarWorkshopManager.Services.Implementations
{
    public class UserRegistrationService : IUserRegistrationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUsernameGeneratorService _usernameGenerator;
        private readonly ILogger<UserRegistrationService> _logger;

        public UserRegistrationService(
            UserManager<ApplicationUser> userManager,
            IUsernameGeneratorService usernameGenerator,
            ILogger<UserRegistrationService> logger)
        {
            _userManager = userManager;
            _usernameGenerator = usernameGenerator;
            _logger = logger;
        }

        public async Task<(IdentityResult result, ApplicationUser user, string token)>
            RegisterUserAsync(string firstName, string lastName, string email, string? phoneNumber, string role)
        {
            _logger.LogInformation("RegisterUserAsync called: {Email}, role={Role}", email, role);
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
            {
                _logger.LogWarning("RegisterUserAsync: CreateAsync failed for {Email}: {Errors}",
                    email, string.Join(";", createResult.Errors.Select(e => e.Description)));
                return (createResult, null!, null!);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                _logger.LogWarning("RegisterUserAsync: AddToRoleAsync failed for {UserId}: {Errors}",
                    user.Id, string.Join(";", roleResult.Errors.Select(e => e.Description)));
                return (roleResult, user, null!);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            _logger.LogInformation("RegisterUserAsync: user created {UserId}", user.Id);
            return (IdentityResult.Success, user, token);
        }
    }
}
