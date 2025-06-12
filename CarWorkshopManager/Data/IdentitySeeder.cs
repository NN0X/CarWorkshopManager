using Microsoft.AspNetCore.Identity;
using CarWorkshopManager.Constants;
using CarWorkshopManager.Models.Identity;

namespace CarWorkshopManager.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in Roles.AllRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        const string adminEmail = "admin123@carworkshop.com";
        const string adminPassword = "FAQm'wNG;1F3qd|u";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            var user = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                FirstName = "System",
                LastName = "Admin",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, adminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, Roles.Admin);
        }
    }
}
