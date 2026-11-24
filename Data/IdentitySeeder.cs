
using Microsoft.AspNetCore.Identity;
using AutoVerifyMVC.Models;
using System.Security.Claims;

namespace AutoVerifyMVC.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles =
            {
                "Buyer",
                "Seller",
                "Inspector",
                "Administrator"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static async Task SeedBootstrapAdministratorAsync(
            IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var email = configuration["BootstrapAdmin:Email"];
            var password = configuration["BootstrapAdmin:Password"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return;

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = "AutoVerify Administrator",
                    EmailConfirmed = true
                };
                var createResult = await userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                    throw new InvalidOperationException("Could not create the bootstrap administrator: "
                        + string.Join("; ", createResult.Errors.Select(error => error.Description)));
            }

            if (!await userManager.IsInRoleAsync(user, "Administrator"))
            {
                var roleResult = await userManager.AddToRoleAsync(user, "Administrator");
                if (!roleResult.Succeeded)
                    throw new InvalidOperationException("Could not assign the Administrator role: "
                        + string.Join("; ", roleResult.Errors.Select(error => error.Description)));
            }

            if (!(await userManager.GetClaimsAsync(user)).Any(claim => claim.Type == "FullName"))
                await userManager.AddClaimAsync(user, new Claim("FullName", user.FullName));
        }
    }
}
