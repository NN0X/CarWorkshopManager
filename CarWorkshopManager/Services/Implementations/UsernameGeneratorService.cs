using CarWorkshopManager.Models.Identity;
using CarWorkshopManager.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CarWorkshopManager.Services.Implementations
{
    public class UsernameGeneratorService : IUsernameGeneratorService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UsernameGeneratorService> _logger;

        public UsernameGeneratorService(
            UserManager<ApplicationUser> userManager,
            ILogger<UsernameGeneratorService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<string> GenerateUsernameAsync(string firstName, string lastName)
        {
            _logger.LogInformation("GenerateUsernameAsync called for {FirstName} {LastName}", firstName, lastName);
            string firstPart = firstName.Length >= 3 ? firstName[..3] : firstName;
            string secondPart = lastName.Length >= 3 ? lastName[..3] : lastName;
            var baseName = (firstPart + secondPart).ToLowerInvariant();

            int suffix = 1;
            string username;
            do
            {
                username = $"{baseName}{suffix}";
                suffix++;
            } while (await _userManager.FindByNameAsync(username) is not null);

            _logger.LogInformation("GenerateUsernameAsync: generated username {Username}", username);
            return username;
        }
    }
}
