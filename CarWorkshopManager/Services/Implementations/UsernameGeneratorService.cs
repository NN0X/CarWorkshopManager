using CarWorkshopManager.Models.Identity;
using CarWorkshopManager.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CarWorkshopManager.Services.Implementations;

public class UsernameGeneratorService : IUsernameGeneratorService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsernameGeneratorService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string> GenerateUsernameAsync(string firstName, string lastName)
    {
        string firstPart = firstName.Length >= 3 ? firstName.Substring(0, 3) : firstName;
        string secondPart = lastName.Length >= 3 ? lastName.Substring(0, 3) : lastName;

        var baseName = (firstPart + secondPart).ToLowerInvariant();

        int suffix = 1;
        string username;

        do
        {
            username = $"{baseName}{suffix}";
            suffix++;
        } while (await _userManager.FindByNameAsync(username) is not null);

        return username;
    }
}
