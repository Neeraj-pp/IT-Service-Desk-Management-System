
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Capstone.API.DTOs;
using Microsoft.Extensions.DependencyInjection;
using HelpDesk.DAL.Model;

public class AuthenticationTests
{
    private static async Task<LoginResponseDto> LoginAsync(
        HttpClient client,
        string username,
        string password)
    {
        var loginRequest = new
        {
            UserName = username,
            Password = password
        };

        var response = await client.PostAsJsonAsync(
            "/api/ITSRPAPI/Login",
            loginRequest);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<LoginResponseDto>();

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));

        return result;
    }

    private static async Task<int> CreateTestRequestAsync(
        CustomWebApplicationFactory factory,
        string username,
        int statusId)
    {
        using var scope = factory.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<HelpDeskDbContext>();

        var status = context.Statuses
            .Single(s => s.StatusId == statusId);

        var request = new ServiceRequest
        {
            Description = "Integration test request",
            Details = "Testing request lifecycle integration.",
            RaisedBy = username,
            RaisedOn = DateTime.UtcNow,
            Justification = "Integration testing",
            ReqStatus = statusId,
            Status = status
        };

        context.ServiceRequests.Add(request);
        await context.SaveChangesAsync();

        return request.RequestId;
    }

    private static async Task AddReopenedAuditAsync(
        CustomWebApplicationFactory factory,
        int requestId)
    {
        using var scope = factory.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<HelpDeskDbContext>();

        context.AuditLogs.Add(new AuditLog
        {
            RequestId = requestId,
            Action = "Reopened",
            PerformedBy = "testuser",
            PerformedAt = DateTime.UtcNow,
            Details = "Previously reopened during testing."
        });

        await context.SaveChangesAsync();
    }


    // Decodes the JWT payload for diagnostic purposes.
    // This does not validate the token signature.
    private static string DecodeJwtPayload(string token)
    {
        var parts = token.Split('.');

        if (parts.Length != 3)
            throw new InvalidOperationException("Invalid JWT format.");

        var payload = parts[1]
            .Replace('-', '+')
            .Replace('_', '/');

        payload = payload.PadRight(
            payload.Length + (4 - payload.Length % 4) % 4,
            '=');

        return Encoding.UTF8.GetString(
            Convert.FromBase64String(payload));
    }

    [Fact]
    public async Task Login_WithValidUserCredentials_ReturnsJwtToken()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.InitializeDatabase();

        using var client = factory.CreateClient();

        var result = await LoginAsync(
            client,
            "testuser",
            "User@123");

        Assert.Equal("Bearer", result.TokenType);
        Assert.Equal("testuser", result.User.UserName);
        Assert.Equal("User", result.User.Role);
        Assert.True(result.ExpiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithValidAdminCredentials_ReturnsAdminRole()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.InitializeDatabase();

        using var client = factory.CreateClient();

        var result = await LoginAsync(
            client,
            "testadmin",
            "Admin@123");

        Assert.Equal("testadmin", result.User.UserName);
        Assert.Equal("Admin", result.User.Role);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.InitializeDatabase();

        using var client = factory.CreateClient();

        var loginRequest = new
        {
            UserName = "testuser",
            Password = "WrongPassword@123"
        };

        var response = await client.PostAsJsonAsync(
            "/api/ITSRPAPI/Login",
            loginRequest);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetAllRequests_WithoutToken_ReturnsUnauthorized()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.InitializeDatabase();

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/ITSRPAPI/GetAllRequest");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetAllRequests_WithUserToken_ReturnsForbidden()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.InitializeDatabase();

        using var client = factory.CreateClient();

        var login = await LoginAsync(
            client,
            "testuser",
            "User@123");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        var response = await client.GetAsync(
            "/api/ITSRPAPI/GetAllRequest");

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task GetAllRequests_WithAdminToken_ReturnsOk()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.InitializeDatabase();

        using var client = factory.CreateClient();

        var login = await LoginAsync(
            client,
            "testadmin",
            "Admin@123");

        // Decode claims for diagnosis.
        var decodedPayload = DecodeJwtPayload(login.AccessToken);

        Console.WriteLine("ADMIN JWT PAYLOAD:");
        Console.WriteLine(decodedPayload);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        var response = await client.GetAsync(
            "/api/ITSRPAPI/GetAllRequest");

        var responseBody = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Login username: {login.User.UserName}");
        Console.WriteLine($"Login role: {login.User.Role}");
        Console.WriteLine($"HTTP status: {(int)response.StatusCode}");
        Console.WriteLine($"Response body: {responseBody}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }


    [Fact]
    public async Task ReopenRequest_WithOwnerTokenAndResolvedRequest_ReturnsOk()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        factory.InitializeDatabase();

        using var client = factory.CreateClient();

        int requestId = await CreateTestRequestAsync(
            factory, "testuser", 2);

        var login = await LoginAsync(
            client, "testuser", "User@123");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer", login.AccessToken);

        var reopenRequest = new
        {
            RequestId = requestId,
            Details = "The original issue still exists.",
            Justification = "The problem was not fully resolved."
        };

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/ITSRPAPI/reopen",
            reopenRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<ServiceRequestResponseDto>();

        Assert.NotNull(result);
        Assert.Equal(3, result.ReqStatus);
    }

    [Fact]
    public async Task ReopenRequest_WithAnotherUsersRequest_ReturnsForbidden()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        factory.InitializeDatabase();

        using var client = factory.CreateClient();

        // Request belongs to Admin, not testuser.
        int requestId = await CreateTestRequestAsync(
            factory, "testadmin", 2);

        var login = await LoginAsync(
            client, "testuser", "User@123");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer", login.AccessToken);

        var reopenRequest = new
        {
            RequestId = requestId,
            Details = "Trying to reopen another user's request.",
            Justification = "Testing request ownership."
        };

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/ITSRPAPI/reopen",
            reopenRequest);

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task ReopenRequest_WithAdminToken_ReturnsForbidden()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        factory.InitializeDatabase();

        using var client = factory.CreateClient();

        int requestId = await CreateTestRequestAsync(
            factory, "testuser", 2);

        var login = await LoginAsync(
            client, "testadmin", "Admin@123");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer", login.AccessToken);

        var reopenRequest = new
        {
            RequestId = requestId,
            Details = "Admin attempting to reopen.",
            Justification = "Testing Admin restriction."
        };

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/ITSRPAPI/reopen",
            reopenRequest);

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task ReopenRequest_WhenAlreadyReopened_ReturnsBadRequest()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        factory.InitializeDatabase();

        using var client = factory.CreateClient();

        int requestId = await CreateTestRequestAsync(
            factory, "testuser", 2);

        // Existing audit entry means reopening was already used.
        await AddReopenedAuditAsync(factory, requestId);

        var login = await LoginAsync(
            client, "testuser", "User@123");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer", login.AccessToken);

        var reopenRequest = new
        {
            RequestId = requestId,
            Details = "Trying to reopen for a second time.",
            Justification = "Testing one-time reopening policy."
        };

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/ITSRPAPI/reopen",
            reopenRequest);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task ReopenRequest_WithNewRequest_ReturnsBadRequest()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        factory.InitializeDatabase();

        using var client = factory.CreateClient();

        int requestId = await CreateTestRequestAsync(
            factory, "testuser", 1);

        var login = await LoginAsync(
            client, "testuser", "User@123");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer", login.AccessToken);

        var reopenRequest = new
        {
            RequestId = requestId,
            Details = "Trying to reopen a new request.",
            Justification = "Testing invalid status transition."
        };

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/ITSRPAPI/reopen",
            reopenRequest);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task ReopenRequest_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        factory.InitializeDatabase();

        using var client = factory.CreateClient();

        int requestId = await CreateTestRequestAsync(
            factory, "testuser", 2);

        var reopenRequest = new
        {
            RequestId = requestId,
            Details = "Unauthenticated reopening attempt.",
            Justification = "Testing authentication requirement."
        };

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/ITSRPAPI/reopen",
            reopenRequest);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

}
