using Microsoft.AspNetCore.Identity;
using StudentIdentityPortal.Constants;
using StudentIdentityPortal.Models;

namespace StudentIdentityPortal.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { RoleNames.Admin, RoleNames.Student })
            if (!await roles.RoleExistsAsync(role))
            {
                var result = await roles.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                    throw new InvalidOperationException($"Could not create the {role} role: " + string.Join("; ", result.Errors.Select(e => e.Description)));
            }

        var email = configuration["Admin:Email"];
        var password = configuration["Admin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;

        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await users.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FirstName = "Nazerke", LastName = "Admin" };
            var created = await users.CreateAsync(admin, password);
            if (!created.Succeeded)
                throw new InvalidOperationException("Could not create the configured admin account: " + string.Join("; ", created.Errors.Select(e => e.Description)));
        }
        if (!await users.IsInRoleAsync(admin, RoleNames.Admin))
        {
            var result = await users.AddToRoleAsync(admin, RoleNames.Admin);
            if (!result.Succeeded)
                throw new InvalidOperationException("Could not assign the Admin role: " + string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }
}
