using CampRating.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampRating.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            context.Database.EnsureCreated();

            // Look for any users
            if (await userManager.Users.AnyAsync())
            {
                return; // DB has been seeded
            }

            // Add roles
            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Add users
            var admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User"
            };
            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, "Admin");

            var user = new ApplicationUser
            {
                UserName = "user",
                Email = "user@example.com",
                FirstName = "Regular",
                LastName = "User"
            };
            await userManager.CreateAsync(user, "User123!");
            await userManager.AddToRoleAsync(user, "User");

            // Add camp places
            var campPlaces = new CampPlace[]
            {
                new CampPlace
                {
                    Name = "Beautiful Mountain Camp",
                    Description = "A stunning campsite with mountain views",
                    Latitude = 42.1234,
                    Longitude = 23.5678,
                    UserId = admin.Id
                },
                new CampPlace
                {
                    Name = "Lakeside Retreat",
                    Description = "Peaceful camping by the lake",
                    Latitude = 42.5678,
                    Longitude = 23.1234,
                    UserId = user.Id
                }
            };
            context.CampPlaces.AddRange(campPlaces);
            await context.SaveChangesAsync();
        }
    }
} 