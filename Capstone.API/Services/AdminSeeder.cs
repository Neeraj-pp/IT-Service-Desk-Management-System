
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

            // ==========================================
            // 1. ENSURE BOTH ROLES EXIST
            // ==========================================

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

                Console.WriteLine("Admin role created.");
            }

            var userRole = await context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "User");

            if (userRole == null)
            {
                userRole = new Role
                {
                    RoleName = "User"
                };

                context.Roles.Add(userRole);
                await context.SaveChangesAsync();

                Console.WriteLine("User role created.");
            }

            // ==========================================
            // 2. ENSURE REQUEST STATUSES EXIST
            // ==========================================

            var expectedStatuses = new Dictionary<int, string>
            {
                { 1, "New" },
                { 2, "Resolved" },
                { 3, "Reopened" }
            };

            var existingStatuses = await context.Statuses
                .ToListAsync();

            // A fresh database should receive statuses in
            // the correct identity order: 1, 2 and 3.
            if (existingStatuses.Count == 0)
            {
                foreach (var status in expectedStatuses)
                {
                    context.Statuses.Add(new Status
                    {
                        Description = status.Value
                    });

                    await context.SaveChangesAsync();

                    Console.WriteLine(
                        $"Status created: {status.Key} - {status.Value}"
                    );
                }
            }
            else
            {
                // Validate existing status IDs before proceeding.
                foreach (var expected in expectedStatuses)
                {
                    var existingStatus = existingStatuses
                        .FirstOrDefault(s =>
                            s.StatusId == expected.Key);

                    if (existingStatus == null)
                    {
                        throw new InvalidOperationException(
                            $"Database is missing StatusId {expected.Key}. " +
                            "Check the existing status records before seeding."
                        );
                    }

                    // Update the old Closed label to Resolved.
                    if (existingStatus.Description != expected.Value)
                    {
                        if (expected.Key == 2 &&
                            existingStatus.Description == "Closed")
                        {
                            existingStatus.Description = "Resolved";
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"Unexpected description for StatusId {expected.Key}. " +
                                $"Expected '{expected.Value}', but found " +
                                $"'{existingStatus.Description}'."
                            );
                        }
                    }
                }

                await context.SaveChangesAsync();

                Console.WriteLine("Request statuses already exist.");
            }

            // ==========================================
            // 3. CHECK WHETHER ADMIN ALREADY EXISTS
            // ==========================================

            bool adminExists = await context.Users
                .AnyAsync(u => u.RoleId == adminRole.RoleId);

            if (adminExists)
            {
                Console.WriteLine(
                    "Admin account already exists."
                );

                return;
            }

            // ==========================================
            // 4. READ INITIAL ADMIN CREDENTIALS
            // ==========================================

            string? userName =
                configuration["AdminSeed:UserName"];

            string? password =
                configuration["AdminSeed:Password"];

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

            // Prevent username conflicts.
            bool usernameExists = await context.Users
                .AnyAsync(u => u.UserName == userName);

            if (usernameExists)
            {
                throw new InvalidOperationException(
                    "The configured Admin username is already used " +
                    "by another account."
                );
            }

            // ==========================================
            // 5. CREATE INITIAL ADMIN ACCOUNT
            // ==========================================

            var adminUser = new User
            {
                UserName = userName,
                CreatedOn = DateTime.UtcNow,
                RoleId = adminRole.RoleId
            };

            adminUser.PasswordHash =
                passwordHasher.HashPassword(
                    adminUser,
                    password
                );

            context.Users.Add(adminUser);

            await context.SaveChangesAsync();

            Console.WriteLine(
                "Initial Admin account created successfully."
            );
        }
    }
}
