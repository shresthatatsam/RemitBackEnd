
using Microsoft.AspNetCore.Identity;

namespace EverestRemit.Api.Data;

public static class UserRoleSeeder
{
    public static async Task AssignRoleAsync(
        IServiceProvider serviceProvider,
        string email,
        string role)
    {
        using var scope = serviceProvider.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<IdentityUser>>();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            throw new InvalidOperationException(
                $"User '{email}' was not found.");
        }

        if (!await roleManager.RoleExistsAsync(role))
        {
            throw new InvalidOperationException(
                $"Role '{role}' does not exist. Seed roles first.");
        }

        if (await userManager.IsInRoleAsync(user, role))
            return;

        var result = await userManager.AddToRoleAsync(user, role);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(e => e.Description));

            throw new InvalidOperationException(
                $"Failed to assign '{role}' to '{email}': {errors}");
        }
    }
}
