
using HelpDesk.DAL.Model;
using HelpDesk.DAL.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public abstract class RepositoryTestBase : IDisposable
{
    protected readonly SqliteConnection Connection;
    protected readonly HelpDeskDbContext Context;
    protected readonly Repository Repository;
    protected readonly PasswordHasher<User> PasswordHasher;

    protected RepositoryTestBase()
    {
        // Create a separate temporary SQLite database for each test.
        Connection = new SqliteConnection("Data Source=:memory:");
        Connection.Open();

        var options = new DbContextOptionsBuilder<HelpDeskDbContext>()
            .UseSqlite(Connection)
            .Options;

        Context = new HelpDeskDbContext(options);

        // Create database tables using the current EF Core model.
        Context.Database.EnsureCreated();

        PasswordHasher = new PasswordHasher<User>();

        Repository = new Repository(Context, PasswordHasher);

        SeedData();
    }

    private void SeedData()
    {
        // Seed application roles.
        Context.Roles.AddRange(
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

        // Seed request statuses.
        Context.Statuses.AddRange(
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

        Context.SaveChanges();
    }

    protected User CreateUser(
        string username = "testuser",
        string password = "Test@123")
    {
        username = username.Trim();

        var existingUser = Context.Users
            .FirstOrDefault(u => u.UserName == username);

        if (existingUser != null)
            return existingUser;

        var user = new User
        {
            UserName = username,
            CreatedOn = DateTime.UtcNow,
            RoleId = 2
        };

        user.PasswordHash =
            PasswordHasher.HashPassword(user, password);

        Context.Users.Add(user);
        Context.SaveChanges();

        return user;
    }

    protected ServiceRequest CreateRequest(
        string username = "testuser",
        string description = "Laptop issue",
        int statusId = 1,
        DateTime? raisedOn = null)
    {
        // Ensure the related user exists before inserting the request.
        CreateUser(username);

        // Ensure the requested status exists.
        var status = Context.Statuses
            .FirstOrDefault(s => s.StatusId == statusId);

        if (status == null)
        {
            throw new InvalidOperationException(
                $"Status with ID {statusId} was not found.");
        }

        var request = new ServiceRequest
        {
            Description = description,
            Details = "Laptop is not starting",
            RaisedBy = username,
            RaisedOn = raisedOn ?? DateTime.Now,
            Justification = "Required for work",
            ReqStatus = statusId,
            Status = status
        };

        Context.ServiceRequests.Add(request);
        Context.SaveChanges();

        return request;
    }

    public void Dispose()
    {
        Context.Dispose();
        Connection.Dispose();
    }
}
