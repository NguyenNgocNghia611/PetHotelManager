namespace PetHotelManager.Data.Seed;

using Microsoft.AspNetCore.Identity;
using PetHotelManager.Models;

public static class DataSeeder
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roleNames = { "Admin", "Staff", "Doctor" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        var adminUser = await userManager.FindByEmailAsync("admin@pethotel.com");
        if (adminUser == null)
        {
            var newAdminUser = new ApplicationUser
            {
                UserName       = "admin",
                Email          = "admin@pethotel.com",
                FullName       = "Quản trị viên",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(newAdminUser, "Admin@123");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdminUser, "Admin");
            }
        }

    }
}