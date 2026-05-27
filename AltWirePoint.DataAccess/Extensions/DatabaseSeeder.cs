using AltWirePoint.DataAccess.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace AltWirePoint.DataAccess.Extensions;

public static class DatabaseSeeder
{
    public static async Task SeedUsersAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        // Admin User
        var adminEmail = "admin@AltWirePoint.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            var adminPassword = configuration["SeedSettings:AdminPassword"] ?? "AdminPassword123!";
            adminUser = new ApplicationUser
            {
                Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479"),
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                Role = "Admin",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                RefreshToken = null,
                RefreshTokenExpirationDateTime = DateTime.MinValue
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to seed admin user: {errors}");
            }
        }

        // Standard User
        var userEmail = "user@AltWirePoint.local";
        var standardUser = await userManager.FindByEmailAsync(userEmail);
        if (standardUser == null)
        {
            var userPassword = configuration["SeedSettings:UserPassword"] ?? "UserPassword123!";
            standardUser = new ApplicationUser
            {
                Id = Guid.Parse("9c8b8e2e-4f3a-4d2e-bf4a-e5c8a1b2c3d4"),
                UserName = userEmail,
                Email = userEmail,
                EmailConfirmed = true,
                Role = "User",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                RefreshToken = null,
                RefreshTokenExpirationDateTime = DateTime.MinValue
            };

            var result = await userManager.CreateAsync(standardUser, userPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to seed standard user: {errors}");
            }
        }
    }
}
