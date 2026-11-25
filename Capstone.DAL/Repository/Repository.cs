
using HelpDesk.DAL.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace HelpDesk.DAL.Repository
{
    /// <summary>
    /// Implements database operations for user authentication,
    /// user registration and service request management.
    /// </summary>
    public class Repository : IRepository
    {
        private readonly HelpDeskDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        /// <summary>
        /// Initializes the repository with the database context
        /// and password hashing service.
        /// </summary>
        /// <param name="context">The Entity Framework database context.</param>
        /// <param name="passwordHasher">The service used to hash and verify passwords.</param>
        public Repository(
            HelpDeskDbContext context,
            IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Authenticates a user by verifying the supplied password
        /// against the stored password hash.
        /// </summary>
        /// <param name="userName">The username of the user.</param>
        /// <param name="password">The supplied password.</param>
        /// <returns>True if authentication succeeds; otherwise, false.</returns>
        public bool Authenticate(string userName, string password)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.UserName == userName);

            if (user == null)
                return false;

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password
            );

            if (result == PasswordVerificationResult.Failed)
                return false;

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash =
                    _passwordHasher.HashPassword(user, password);

                _context.SaveChanges();
            }

            return true;
        }

        /// <summary>
        /// Registers a new user with a securely hashed password
        /// and the default User role.
        /// </summary>
        /// <param name="userName">The username for the new account.</param>
        /// <param name="password">The password to hash and store.</param>
        /// <returns>True if registration succeeds; otherwise, false.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the User role is not available in the database.
        /// </exception>
        public bool RegisterUser(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName) ||
                string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            userName = userName.Trim();

            // Check whether username already exists.
            bool userExists = _context.Users
                .Any(u => u.UserName == userName);

            if (userExists)
                return false;

            // Retrieve the User role from the database.
            var userRole = _context.Roles
                .FirstOrDefault(r => r.RoleName == "User");

            if (userRole == null)
            {
                throw new InvalidOperationException(
                    "User role was not found in the database."
                );
            }

            var newUser = new User
            {
                UserName = userName,
                CreatedOn = DateTime.UtcNow,
                RoleId = userRole.RoleId
            };

            newUser.PasswordHash =
                _passwordHasher.HashPassword(newUser, password);

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return true;
        }

        /// <summary>
        /// Retrieves all service requests, including their status information.
        /// </summary>
        /// <returns>A list of service requests.</returns>
        public List<ServiceRequest> ViewRequests()
        {
            return _context.ServiceRequests
                .Include(s => s.Status)
                .ToList();
        }

        /// <summary>
        /// Retrieves service requests raised by a specific user.
        /// </summary>
        /// <param name="userName">The username used to filter requests.</param>
        /// <returns>A list of matching service requests.</returns>
        public List<ServiceRequest> ViewRequests(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return new List<ServiceRequest>();

            return _context.ServiceRequests
                .Include(s => s.Status)
                .Where(r => r.RaisedBy == userName)
                .ToList();
        }

        /// <summary>
        /// Creates a new service request and assigns its initial status.
        /// </summary>
        /// <param name="newRequest">The service request to create.</param>
        /// <returns>The identifier of the newly created request.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the supplied request is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the initial request status is unavailable.
        /// </exception>
        public int RaiseRequest(ServiceRequest newRequest)
        {
            if (newRequest == null)
                throw new ArgumentNullException(nameof(newRequest));

            // Validate that New status exists.
            var newStatus = _context.Statuses
                .FirstOrDefault(s => s.StatusId == 1);

            if (newStatus == null)
                throw new InvalidOperationException(
                    "New status was not found in the database.");

            newRequest.ReqStatus = newStatus.StatusId;
            newRequest.Status = newStatus;

            _context.ServiceRequests.Add(newRequest);
            _context.SaveChanges();

            return newRequest.RequestId;
        }

        /// <summary>
        /// Retrieves a service request using its unique identifier.
        /// </summary>
        /// <param name="requestId">The service request identifier.</param>
        /// <returns>The matching service request, or null if not found.</returns>
        public ServiceRequest? GetRequestById(int requestId)
        {
            return _context.ServiceRequests
                .Include(s => s.Status)
                .FirstOrDefault(r => r.RequestId == requestId);
        }

        /// <summary>
        /// Updates an existing service request and changes its status to Reopened.
        /// </summary>
        /// <param name="request">The updated service request information.</param>
        /// <returns>True if the request was reopened; otherwise, false.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the Reopened status is unavailable.
        /// </exception>

        public bool ReOpenRequest(ServiceRequest request)
        {
            if (request == null)
                return false;

            var existing = _context.ServiceRequests
                .FirstOrDefault(r => r.RequestId == request.RequestId);

            if (existing == null)
                return false;

            // Only a Resolved request (StatusId = 2) can be reopened.
            if (existing.ReqStatus != 2)
                return false;

            var reopenedStatus = _context.Statuses
                .FirstOrDefault(s => s.StatusId == 3);

            if (reopenedStatus == null)
                throw new InvalidOperationException(
                    "Reopened status was not found in the database.");

            existing.Description = request.Description;
            existing.Details = request.Details;
            existing.Justification = request.Justification;
            existing.ReqStatus = reopenedStatus.StatusId;
            existing.Status = reopenedStatus;

            _context.SaveChanges();

            return true;
        }


        /// <summary>
        /// Retrieves service requests associated with a specific user.
        /// </summary>
        /// <param name="userName">The username used to retrieve requests.</param>
        /// <returns>A list of matching service requests.</returns>
        public List<ServiceRequest> GetRequestBySP(string userName)
        {
            return ViewRequests(userName);
        }

        /// <summary>
        /// Changes the status of an existing service request to Closed.
        /// </summary>
        /// <param name="requestId">The identifier of the request to close.</param>
        /// <returns>True if the request was closed; otherwise, false.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the Closed status is unavailable.
        /// </exception>

        public bool CloseRequest(int requestId)
        {
            var request = _context.ServiceRequests
                .FirstOrDefault(r => r.RequestId == requestId);

            if (request == null)
                return false;

            // Admin can resolve only New (1) or Reopened (3) requests.
            if (request.ReqStatus != 1 && request.ReqStatus != 3)
                return false;

            var resolvedStatus = _context.Statuses
                .FirstOrDefault(s => s.StatusId == 2);

            if (resolvedStatus == null)
                throw new InvalidOperationException(
                    "Resolved status was not found in the database.");

            request.ReqStatus = resolvedStatus.StatusId;
            request.Status = resolvedStatus;

            _context.SaveChanges();

            return true;
        }


        /// <summary>
        /// Deletes a service request using its identifier.
        /// </summary>
        /// <param name="requestId">The identifier of the request to delete.</param>
        /// <returns>True if the request was deleted; otherwise, false.</returns>
        public bool DeleteRequest(int requestId)
        {
            var request = _context.ServiceRequests
                .FirstOrDefault(r => r.RequestId == requestId);

            if (request == null)
                return false;

            _context.ServiceRequests.Remove(request);
            _context.SaveChanges();

            return true;
        }

        /// <summary>
        /// Retrieves a user and their associated role using the username.
        /// </summary>
        /// <param name="userName">The username to search for.</param>
        /// <returns>The matching user, or null if not found.</returns>
        public User? GetUser(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return null;

            return _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.UserName == userName);
        }

        /// <summary>
        /// Retrieves service requests using pagination.
        /// </summary>
        /// <param name="pageNumber">The requested page number.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <returns>
        /// A tuple containing the requested records and total record count.
        /// </returns>
        public (List<ServiceRequest> Items, int TotalCount)
            GetRequestsPaged(int pageNumber, int pageSize)
        {
            var query = _context.ServiceRequests
                .AsNoTracking()
                .OrderByDescending(r => r.RequestId);

            int totalCount = query.Count();

            var items = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (items, totalCount);
        }

        /// <summary>
        /// Retrieves service requests using pagination and optional filters.
        /// Supports searching by description or details and filtering by
        /// status, date range and username.
        /// </summary>
        /// <param name="pageNumber">The requested page number.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="searchTerm">Optional search text.</param>
        /// <param name="statusId">Optional request status filter.</param>
        /// <param name="fromDate">Optional starting date.</param>
        /// <param name="toDate">Optional ending date, inclusive.</param>
        /// <param name="userName">Optional username filter.</param>
        /// <returns>
        /// A tuple containing the filtered records and total matching record count.
        /// </returns>
        public (List<ServiceRequest> Items, int TotalCount)
            GetRequestsPaged(
                int pageNumber,
                int pageSize,
                string? searchTerm,
                int? statusId,
                DateTime? fromDate,
                DateTime? toDate,
                string? userName)
        {
            var query = _context.ServiceRequests
                .AsNoTracking()
                .AsQueryable();

            // Search by description or details
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string search = searchTerm.Trim();

                query = query.Where(r =>
                    r.Description.Contains(search) ||
                    r.Details.Contains(search));
            }

            // Filter by request status
            if (statusId.HasValue)
            {
                query = query.Where(r => r.ReqStatus == statusId.Value);
            }

            // Filter by start date
            if (fromDate.HasValue)
            {
                query = query.Where(r =>
                    r.RaisedOn >= fromDate.Value.Date);
            }

            // Filter by end date (inclusive)
            if (toDate.HasValue)
            {
                DateTime endDateExclusive = toDate.Value.Date.AddDays(1);

                query = query.Where(r =>
                    r.RaisedOn < endDateExclusive);
            }

            // Filter by username
            if (!string.IsNullOrWhiteSpace(userName))
            {
                string username = userName.Trim();

                query = query.Where(r =>
                    r.RaisedBy == username);
            }

            // Count after applying filters
            int totalCount = query.Count();

            // Apply sorting and pagination
            var items = query
                .OrderByDescending(r => r.RequestId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (items, totalCount);
        }

        /// <summary>
        /// Retrieves the total number of service requests grouped by status.
        /// </summary>
        /// <returns>Total, New, Reopened and Closed request counts.</returns>
        public (int Total, int New, int Reopened, int Closed)
            GetRequestStatusCounts()
        {
            int total = _context.ServiceRequests.Count();

            int newRequests = _context.ServiceRequests
                .Count(r => r.ReqStatus == 1);

            int closedRequests = _context.ServiceRequests
                .Count(r => r.ReqStatus == 2);

            int reopenedRequests = _context.ServiceRequests
                .Count(r => r.ReqStatus == 3);

            return (
                total,
                newRequests,
                reopenedRequests,
                closedRequests
            );
        }
    }

}
