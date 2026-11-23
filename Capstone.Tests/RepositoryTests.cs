
using HelpDesk.DAL.Model;

public class RepositoryTests : RepositoryTestBase
{
    [Fact]
    public void RegisterUser_WithValidDetails_ReturnsTrue()
    {
        // Act
        bool result = Repository.RegisterUser("newuser", "Password@123");

        // Assert
        Assert.True(result);

        var savedUser = Context.Users
            .FirstOrDefault(u => u.UserName == "newuser");

        Assert.NotNull(savedUser);
        Assert.NotEqual("Password@123", savedUser.PasswordHash);
        Assert.Equal(2, savedUser.RoleId);
    }

    [Fact]
    public void RegisterUser_WithExistingUsername_ReturnsFalse()
    {
        // Arrange
        CreateUser("existinguser");

        // Act
        bool result = Repository.RegisterUser(
            "existinguser", "Another@123");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Authenticate_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        CreateUser("testuser", "Test@123");

        // Act
        bool result = Repository.Authenticate("testuser", "Test@123");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Authenticate_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        CreateUser("testuser", "Test@123");

        // Act
        bool result = Repository.Authenticate("testuser", "Wrong@123");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RaiseRequest_WithValidRequest_SavesRequest()
    {
        // Arrange
        var request = new ServiceRequest
        {
            Description = "Network issue",
            Details = "Internet is not working",
            RaisedBy = "testuser",
            RaisedOn = DateTime.Now,
            Justification = "Required for work"
        };

        // Act
        int requestId = Repository.RaiseRequest(request);

        // Assert
        Assert.True(requestId > 0);

        var savedRequest = Context.ServiceRequests
            .FirstOrDefault(r => r.RequestId == requestId);

        Assert.NotNull(savedRequest);
        Assert.Equal(1, savedRequest.ReqStatus);
    }

    [Fact]
    public void CloseRequest_WithExistingRequest_ChangesStatusToClosed()
    {
        // Arrange
        var request = CreateRequest();

        // Act
        bool result = Repository.CloseRequest(request.RequestId);

        // Assert
        Assert.True(result);

        var updatedRequest = Repository.GetRequestById(request.RequestId);

        Assert.NotNull(updatedRequest);
        Assert.Equal(2, updatedRequest.ReqStatus);
    }

    [Fact]
    public void ReOpenRequest_WithExistingRequest_ChangesStatusToReopened()
    {
        // Arrange
        var request = CreateRequest(statusId: 2);

        request.Description = "Updated laptop issue";
        request.Details = "Laptop still does not start";
        request.Justification = "Work is blocked";

        // Act
        bool result = Repository.ReOpenRequest(request);

        // Assert
        Assert.True(result);

        var updatedRequest = Repository.GetRequestById(request.RequestId);

        Assert.NotNull(updatedRequest);
        Assert.Equal(3, updatedRequest.ReqStatus);
        Assert.Equal("Updated laptop issue", updatedRequest.Description);
    }

    [Fact]
    public void DeleteRequest_WithExistingRequest_RemovesRequest()
    {
        // Arrange
        var request = CreateRequest();

        // Act
        bool result = Repository.DeleteRequest(request.RequestId);

        // Assert
        Assert.True(result);

        var deletedRequest = Repository.GetRequestById(request.RequestId);

        Assert.Null(deletedRequest);
    }

    [Fact]
    public void ViewRequests_WithUsername_ReturnsOnlyThatUsersRequests()
    {
        // Arrange
        CreateRequest(username: "user1", description: "User 1 issue");
        CreateRequest(username: "user2", description: "User 2 issue");
        CreateRequest(username: "user1", description: "Another User 1 issue");

        // Act
        var results = Repository.ViewRequests("user1");

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, request =>
            Assert.Equal("user1", request.RaisedBy));
    }


    [Fact]
    public void GetRequestsPaged_WithSearchTerm_ReturnsMatchingRequests()
    {
        // Arrange
        CreateRequest(
            description: "Laptop problem");

        CreateRequest(
            description: "Printer problem");

        CreateRequest(
            description: "Laptop replacement");

        // Act
        var (items, totalCount) = Repository.GetRequestsPaged(
            pageNumber: 1,
            pageSize: 10,
            searchTerm: "Laptop",
            statusId: null,
            fromDate: null,
            toDate: null,
            userName: null
        );

        // Assert
        Assert.Equal(3, totalCount);
        Assert.Equal(3, items.Count);

        Assert.All(items, request =>
            Assert.True(
                request.Description.Contains("Laptop") ||
                request.Details.Contains("Laptop")));
    }


    [Fact]
    public void GetRequestsPaged_ReturnsOnlyRequestedPage()
    {
        // Arrange
        CreateRequest(description: "Request 1");
        CreateRequest(description: "Request 2");
        CreateRequest(description: "Request 3");

        // Act
        var (items, totalCount) = Repository.GetRequestsPaged(
            pageNumber: 1,
            pageSize: 2
        );

        // Assert
        Assert.Equal(3, totalCount);
        Assert.Equal(2, items.Count);
    }


    [Fact]
    public void RegisterUser_WithBlankUsername_ReturnsFalse()
    {
        // Act
        bool result = Repository.RegisterUser(" ", "Password@123");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetRequestById_WithNonExistingId_ReturnsNull()
    {
        // Act
        var result = Repository.GetRequestById(9999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void CloseRequest_WithNonExistingId_ReturnsFalse()
    {
        // Act
        bool result = Repository.CloseRequest(9999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ReOpenRequest_WithNonExistingRequest_ReturnsFalse()
    {
        // Arrange
        var request = new ServiceRequest
        {
            RequestId = 9999,
            Description = "Test issue",
            Details = "Test details",
            RaisedBy = "testuser",
            RaisedOn = DateTime.Now,
            Justification = "Testing"
        };

        // Act
        bool result = Repository.ReOpenRequest(request);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void DeleteRequest_WithNonExistingId_ReturnsFalse()
    {
        // Act
        bool result = Repository.DeleteRequest(9999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetUser_WithNonExistingUsername_ReturnsNull()
    {
        // Act
        var result = Repository.GetUser("unknownuser");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ViewRequests_WithBlankUsername_ReturnsEmptyList()
    {
        // Act
        var result = Repository.ViewRequests(" ");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetRequestsPaged_WithStatusFilter_ReturnsOnlyMatchingStatus()
    {
        // Arrange
        CreateRequest(description: "New request", statusId: 1);
        CreateRequest(description: "Closed request", statusId: 2);
        CreateRequest(description: "Another new request", statusId: 1);

        // Act
        var (items, totalCount) = Repository.GetRequestsPaged(
            pageNumber: 1,
            pageSize: 10,
            searchTerm: null,
            statusId: 2,
            fromDate: null,
            toDate: null,
            userName: null
        );

        // Assert
        Assert.Equal(1, totalCount);
        Assert.Single(items);
        Assert.All(items, request =>
            Assert.Equal(2, request.ReqStatus));
    }

    [Fact]
    public void GetRequestsPaged_WithUsernameFilter_ReturnsOnlyMatchingUser()
    {
        // Arrange
        CreateRequest(username: "user1", description: "Issue one");
        CreateRequest(username: "user2", description: "Issue two");
        CreateRequest(username: "user1", description: "Issue three");

        // Act
        var (items, totalCount) = Repository.GetRequestsPaged(
            pageNumber: 1,
            pageSize: 10,
            searchTerm: null,
            statusId: null,
            fromDate: null,
            toDate: null,
            userName: "user1"
        );

        // Assert
        Assert.Equal(2, totalCount);
        Assert.Equal(2, items.Count);
        Assert.All(items, request =>
            Assert.Equal("user1", request.RaisedBy));
    }

    [Fact]
    public void GetRequestsPaged_WithDateRange_ReturnsOnlyRequestsWithinRange()
    {
        // Arrange
        CreateRequest(
            description: "Older request",
            raisedOn: new DateTime(2026, 1, 10, 10, 0, 0));

        CreateRequest(
            description: "First matching request",
            raisedOn: new DateTime(2026, 2, 15, 9, 0, 0));

        CreateRequest(
            description: "Second matching request",
            raisedOn: new DateTime(2026, 2, 20, 15, 0, 0));

        CreateRequest(
            description: "Newer request",
            raisedOn: new DateTime(2026, 3, 10, 10, 0, 0));

        // Act
        var (items, totalCount) = Repository.GetRequestsPaged(
            pageNumber: 1,
            pageSize: 10,
            searchTerm: null,
            statusId: null,
            fromDate: new DateTime(2026, 2, 15),
            toDate: new DateTime(2026, 2, 20),
            userName: null
        );

        // Assert
        Assert.Equal(2, totalCount);
        Assert.Equal(2, items.Count);
        Assert.All(items, request =>
        {
            Assert.True(request.RaisedOn >= new DateTime(2026, 2, 15));
            Assert.True(request.RaisedOn < new DateTime(2026, 2, 21));
        });
    }

    [Fact]
    public void GetRequestsPaged_SecondPage_ReturnsRemainingRecords()
    {
        // Arrange
        CreateRequest(description: "Request one");
        CreateRequest(description: "Request two");
        CreateRequest(description: "Request three");
        CreateRequest(description: "Request four");
        CreateRequest(description: "Request five");

        // Act
        var (items, totalCount) = Repository.GetRequestsPaged(
            pageNumber: 2,
            pageSize: 2
        );

        // Assert
        Assert.Equal(5, totalCount);
        Assert.Equal(2, items.Count);

        // Requests are sorted by descending RequestId.
        Assert.True(items[0].RequestId > items[1].RequestId);
    }


    [Fact]
    public void CloseRequest_WithNewRequest_ChangesStatusToResolved()
    {
        // Arrange
        var request = CreateRequest(statusId: 1);

        // Act
        bool result = Repository.CloseRequest(request.RequestId);

        // Assert
        Assert.True(result);
        Assert.Equal(2, Repository.GetRequestById(request.RequestId)!.ReqStatus);
    }

    [Fact]
    public void CloseRequest_WithReopenedRequest_ChangesStatusToResolved()
    {
        // Arrange
        var request = CreateRequest(statusId: 3);

        // Act
        bool result = Repository.CloseRequest(request.RequestId);

        // Assert
        Assert.True(result);
        Assert.Equal(2, Repository.GetRequestById(request.RequestId)!.ReqStatus);
    }

    [Fact]
    public void CloseRequest_WithAlreadyResolvedRequest_ReturnsFalse()
    {
        // Arrange
        var request = CreateRequest(statusId: 2);

        // Act
        bool result = Repository.CloseRequest(request.RequestId);

        // Assert
        Assert.False(result);
        Assert.Equal(2, Repository.GetRequestById(request.RequestId)!.ReqStatus);
    }

    [Fact]
    public void ReOpenRequest_WithNewRequest_ReturnsFalse()
    {
        // Arrange
        var request = CreateRequest(statusId: 1);

        request.Details = "Updated laptop details";
        request.Justification = "Issue still exists";

        // Act
        bool result = Repository.ReOpenRequest(request);

        // Assert
        Assert.False(result);
        Assert.Equal(1, Repository.GetRequestById(request.RequestId)!.ReqStatus);
    }

    [Fact]
    public void ReOpenRequest_WithAlreadyReopenedRequest_ReturnsFalse()
    {
        // Arrange
        var request = CreateRequest(statusId: 3);

        request.Details = "Updated laptop details";
        request.Justification = "Issue still exists";

        // Act
        bool result = Repository.ReOpenRequest(request);

        // Assert
        Assert.False(result);
        Assert.Equal(3, Repository.GetRequestById(request.RequestId)!.ReqStatus);
    }


}
