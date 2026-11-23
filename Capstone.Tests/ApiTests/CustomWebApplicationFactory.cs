
using Capstone.API.Configuration;
using HelpDesk.DAL.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public CustomWebApplicationFactory()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, configuration) =>
        {
            var testSettings = new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "CapstoneTestIssuer",
                ["Jwt:Audience"] = "CapstoneTestAudience",
                ["Jwt:Key"] = "Capstone_Test_Key_12345678901234567890",
                ["Jwt:AccessTokenMinutes"] = "30",

                // Original Program.cs requires a supported provider.
                // The test DbContext registration below replaces SQL Server with SQLite.
                ["Database:Provider"] = "SqlServer",
                ["ConnectionStrings:DefaultConnection"] =
                    "Server=(localdb)\\MSSQLLocalDB;Database=CapstoneTest;Trusted_Connection=True;"
            };

            configuration.AddInMemoryCollection(testSettings);
        });

        builder.ConfigureTestServices(services =>
        {
            // Remove the production database configuration.
            services.RemoveAll<DbContextOptions<HelpDeskDbContext>>();
            services.RemoveAll<HelpDeskDbContext>();
            services.RemoveAll<
                IDbContextOptionsConfiguration<HelpDeskDbContext>>();

            // Use a shared SQLite in-memory connection for integration tests.
            services.AddDbContext<HelpDeskDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // Configure JWT token generation for tests.
            services.PostConfigure<JwtOptions>(options =>
            {
                options.Issuer = "CapstoneTestIssuer";
                options.Audience = "CapstoneTestAudience";
                options.Key =
                    "Capstone_Test_Key_12345678901234567890";
                options.AccessTokenMinutes = 30;
            });

            // Configure JWT validation with the same test settings.
            services.PostConfigure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    const string testIssuer = "CapstoneTestIssuer";
                    const string testAudience = "CapstoneTestAudience";
                    const string testKey =
                        "Capstone_Test_Key_12345678901234567890";

                    options.TokenValidationParameters.ValidIssuer =
                        testIssuer;

                    options.TokenValidationParameters.ValidAudience =
                        testAudience;

                    options.TokenValidationParameters.IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(testKey));

                    options.TokenValidationParameters.ValidateIssuer = true;
                    options.TokenValidationParameters.ValidateAudience = true;
                    options.TokenValidationParameters.ValidateIssuerSigningKey = true;
                    options.TokenValidationParameters.ValidateLifetime = true;

                    // Match production JWT claim mapping.
                    options.TokenValidationParameters.NameClaimType =
                        ClaimTypes.Name;

                    options.TokenValidationParameters.RoleClaimType =
                        ClaimTypes.Role;
                });
        });
    }

    public void InitializeDatabase()
    {
        using var scope = Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<HelpDeskDbContext>();

        context.Database.EnsureCreated();

        // Seed roles.
        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                new Role
                {
                    RoleId = 1,
                    RoleName = "Admin"
                },
                new Role
                {
                    RoleId = 2,
                    RoleName = "User"
                }
            );
        }

        // Seed request statuses.
        if (!context.Statuses.Any())
        {
            context.Statuses.AddRange(
                new Status
                {
                    StatusId = 1,
                    Description = "New"
                },
                new Status
                {
                    StatusId = 2,
                    Description = "Closed"
                },
                new Status
                {
                    StatusId = 3,
                    Description = "Reopened"
                }
            );
        }

        context.SaveChanges();

        var passwordHasher = new PasswordHasher<User>();

        // Create test accounts.
        AddTestUser(
            context,
            passwordHasher,
            "testadmin",
            "Admin@123",
            1);

        AddTestUser(
            context,
            passwordHasher,
            "testuser",
            "User@123",
            2);

        context.SaveChanges();
    }

    private static void AddTestUser(
        HelpDeskDbContext context,
        PasswordHasher<User> passwordHasher,
        string username,
        string password,
        int roleId)
    {
        if (context.Users.Any(u => u.UserName == username))
        {
            return;
        }

        var user = new User
        {
            UserName = username,
            CreatedOn = DateTime.UtcNow,
            RoleId = roleId
        };

        user.PasswordHash =
            passwordHasher.HashPassword(user, password);

        context.Users.Add(user);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
