
using HelpDesk.DAL.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Capstone.API.Services
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<HelpDeskDbContext>();

            var passwordHasher = scope.ServiceProvider
                .GetRequiredService<IPasswordHasher<User>>();

            // Find or create the Admin role.
            var adminRole = await context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "Admin");

            if (adminRole == null)
            {
                adminRole = new Role
                {
                    RoleName = "Admin"
                };

                context.Roles.Add(adminRole);
                await context.SaveChangesAsync();
            }

            // Do not create another Admin if one already exists.
            bool adminExists = await context.Users
                .AnyAsync(u => u.RoleId == adminRole.RoleId);

            if (adminExists)
            {
                Console.WriteLine("Admin account already exists.");
                return;
            }

            // Read initial Admin credentials from configuration.
            string? userName = configuration["AdminSeed:UserName"];
            string? password = configuration["AdminSeed:Password"];

            if (string.IsNullOrWhiteSpace(userName) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "Admin account does not exist. Configure " +
                    "AdminSeed:UserName and AdminSeed:Password."
                );
            }

            userName = userName.Trim();

            if (userName.Length > 20)
            {
                throw new InvalidOperationException(
                    "Admin username cannot exceed 20 characters."
                );
            }

            if (password.Length < 12)
            {
                throw new InvalidOperationException(
                    "Initial Admin password must contain at least 12 characters."
                );
            }

            // Prevent username conflicts with existing accounts.
            bool usernameExists = await context.Users
                .AnyAsync(u => u.UserName == userName);

            if (usernameExists)
            {
                throw new InvalidOperationException(
                    "The configured Admin username is already used by another account."
                );
            }

            var adminUser = new User
            {
                UserName = userName,
                CreatedOn = DateTime.UtcNow,
                RoleId = adminRole.RoleId
            };

            adminUser.PasswordHash =
                passwordHasher.HashPassword(adminUser, password);

            context.Users.Add(adminUser);

            await context.SaveChangesAsync();

            Console.WriteLine("Initial Admin account created successfully.");
        }
    }
}
