
using HelpDesk.DAL.Model;
using HelpDesk.DAL.Repository;
using Microsoft.AspNetCore.Mvc;
using Capstone.API.DTOs;
using Capstone.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Capstone.API.Controllers
{
    /// <summary>
    /// Provides APIs for user authentication, service request management,
    /// user profile management, and administrative audit operations.
    /// </summary>
    /// <remarks>
    /// JWT Bearer authentication is required by default.
    /// Registration and login are publicly accessible.
    /// Administrative endpoints require the Admin role.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ITSRPAPIController : ControllerBase
    {
        private readonly IRepository _repository;
        private readonly JwtTokenService _jwtTokenService;
        private readonly AuditLogService _auditLogService;
        private readonly HelpDeskDbContext _context;

        public ITSRPAPIController(
            IRepository repository,
            JwtTokenService jwtTokenService,
            AuditLogService auditLogService,
            HelpDeskDbContext context)
        {
            _repository = repository;
            _jwtTokenService = jwtTokenService;
            _auditLogService = auditLogService;
            _context = context;
        }

        // ==========================================
        // USER REGISTRATION
        // ==========================================
        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <remarks>
        /// New accounts are assigned the User role by the backend.
        /// </remarks>
        /// <param name="request">Username and password for the new account.</param>
        /// <returns>Registration confirmation and the assigned role.</returns>
        /// <response code="201">User registered successfully.</response>
        /// <response code="400">Invalid registration details.</response>
        /// <response code="409">Username already exists.</response>
        [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(
            [FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string userName = request.UserName.Trim();

            if (string.IsNullOrWhiteSpace(userName) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and password are required.");
            }

            bool registered = _repository.RegisterUser(
                userName,
                request.Password
            );

            if (!registered)
                return Conflict("Username already exists.");

            var response = new RegisterResponseDto
            {
                Message = "Registration successful.",
                UserName = userName,
                Role = "User"
            };

            return StatusCode(201, response);
        }

        // ==========================================
        // JWT LOGIN
        // ==========================================
        /// <summary>
        /// Authenticates a user and generates a JWT access token.
        /// </summary>
        /// <param name="request">User login credentials.</param>
        /// <returns>JWT access token, expiry time, username and role.</returns>
        /// <response code="200">Authentication successful.</response>
        /// <response code="400">Invalid login request.</response>
        /// <response code="401">Invalid username or password.</response>
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(
            [FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string userName = request.UserName.Trim();

            if (string.IsNullOrWhiteSpace(userName) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and password are required.");
            }

            bool isAuthenticated = _repository.Authenticate(
                userName,
                request.Password
            );

            if (!isAuthenticated)
                return Unauthorized("Invalid username or password.");

            var user = _repository.GetUser(userName);

            if (user == null || user.Role == null)
                return Unauthorized("Invalid username or password.");

            var tokenResult = _jwtTokenService.CreateToken(user);

            var response = new LoginResponseDto
            {
                AccessToken = tokenResult.Token,
                TokenType = "Bearer",
                ExpiresAtUtc = tokenResult.ExpiresAtUtc,

                User = new LoginUserDto
                {
                    UserName = user.UserName,
                    Role = user.Role.RoleName
                }
            };

            return Ok(response);
        }

        // ==========================================
        // GET ALL SERVICE REQUESTS - ADMIN ONLY
        // ==========================================
        /// <summary>
        /// Retrieves all service requests using pagination and optional filters.
        /// </summary>
        /// <remarks>
        /// This endpoint is restricted to administrators.
        /// Filters include search term, status, date range and username.
        /// </remarks>
        /// <param name="pageNumber">Page number, starting from 1.</param>
        /// <param name="pageSize">Number of records per page (1–100).</param>
        /// <param name="searchTerm">Optional search across request description and details.</param>
        /// <param name="statusId">Optional service request status ID.</param>
        /// <param name="fromDate">Optional start date filter.</param>
        /// <param name="toDate">Optional end date filter.</param>
        /// <param name="userName">Optional username filter.</param>
        /// <returns>A paginated collection of service requests.</returns>
        /// <response code="200">Requests retrieved successfully.</response>
        /// <response code="400">Invalid pagination or filter parameters.</response>
        /// <response code="401">Authentication is required.</response>
        /// <response code="403">Administrator role is required.</response>
        [ProducesResponseType(typeof(PagedResponseDto<ServiceRequestResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllRequest")]
        public IActionResult GetAllRequest(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            int? statusId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? userName = null)
        {
            if (pageNumber < 1)
                return BadRequest("Page number must be at least 1.");

            if (pageSize < 1 || pageSize > 100)
                return BadRequest("Page size must be between 1 and 100.");

            if (searchTerm?.Length > 100)
                return BadRequest("Search term cannot exceed 100 characters.");

            if (userName?.Length > 20)
                return BadRequest("Username cannot exceed 20 characters.");

            if (statusId.HasValue)
            {
                if (statusId.Value <= 0)
                    return BadRequest("Invalid status ID.");

                bool statusExists = _context.Statuses
                    .Any(s => s.StatusId == statusId.Value);

                if (!statusExists)
                    return BadRequest("The specified status does not exist.");
            }

            if (fromDate.HasValue &&
                toDate.HasValue &&
                fromDate.Value.Date > toDate.Value.Date)
            {
                return BadRequest("From date cannot be after To date.");
            }

            searchTerm = string.IsNullOrWhiteSpace(searchTerm)
                ? null
                : searchTerm.Trim();

            userName = string.IsNullOrWhiteSpace(userName)
                ? null
                : userName.Trim();

            var result = _repository.GetRequestsPaged(
                pageNumber,
                pageSize,
                searchTerm,
                statusId,
                fromDate,
                toDate,
                userName
            );

            var response = new PagedResponseDto<ServiceRequestResponseDto>
            {
                Items = result.Items.Select(r =>
                    new ServiceRequestResponseDto
                    {
                        RequestId = r.RequestId,
                        Description = r.Description,
                        Details = r.Details,
                        RaisedBy = r.RaisedBy,
                        RaisedOn = r.RaisedOn,
                        Justification = r.Justification,
                        ReqStatus = r.ReqStatus
                    }).ToList(),

                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = result.TotalCount,
                TotalPages = (int)Math.Ceiling(
                    result.TotalCount / (double)pageSize
                )
            };

            return Ok(response);
        }

        // ==========================================
        // GET REQUEST STATUS COUNTS - ADMIN ONLY
        // ==========================================

        /// <summary>
        /// Retrieves total, new, reopened and closed service request counts.
        /// </summary>
        /// <returns>Request status summary.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "Admin")]
        [HttpGet("GetRequestStatusCounts")]
        public IActionResult GetRequestStatusCounts()
        {
            var counts = _repository.GetRequestStatusCounts();

            return Ok(new
            {
                total = counts.Total,
                newRequests = counts.New,
                reopened = counts.Reopened,
                closed = counts.Closed
            });
        }

        // ==========================================
        // GET REQUESTS BY USERNAME
        // ==========================================
        /// <summary>
        /// Retrieves service requests belonging to a user.
        /// </summary>
        /// <remarks>
        /// Regular users can retrieve only their own requests.
        /// Administrators can retrieve requests for any username.
        /// </remarks>
        /// <param name="userName">Username to filter by. Required for administrators.</param>
        /// <returns>A collection of service requests.</returns>
        /// <response code="200">Requests retrieved successfully.</response>
        /// <response code="400">Username is missing or invalid for an administrator.</response>
        /// <response code="401">Authentication is required.</response>
        [ProducesResponseType(typeof(List<ServiceRequestResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("GetRequestByuserName")]
        public IActionResult GetRequestByUN(string? userName = null)
        {
            string? loggedInUser = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(loggedInUser))
                return Unauthorized();

            List<ServiceRequest> requests;

            // Admin can view requests for any username.
            if (User.IsInRole("Admin"))
            {
                if (string.IsNullOrWhiteSpace(userName))
                    return BadRequest("Username is required.");

                userName = userName.Trim();

                if (userName.Length > 20)
                    return BadRequest("Username cannot exceed 20 characters.");

                requests = _repository.ViewRequests(userName);
            }
            else
            {
                // Normal users can view only their own requests.
                requests = _repository.ViewRequests(loggedInUser);
            }

            var response = requests.Select(r =>
                new ServiceRequestResponseDto
                {
                    RequestId = r.RequestId,
                    Description = r.Description,
                    Details = r.Details,
                    RaisedBy = r.RaisedBy,
                    RaisedOn = r.RaisedOn,
                    Justification = r.Justification,
                    ReqStatus = r.ReqStatus
                }).ToList();

            return Ok(response);
        }

        // ==========================================
        // REOPEN SERVICE REQUEST
        // ==========================================
        /// <summary>
        /// Reopens an existing service request.
        /// </summary>
        /// <remarks>
        /// Users can reopen their own requests. Administrators can reopen any request.
        /// The operation is recorded in the audit log.
        /// </remarks>
        /// <param name="request">Request ID, updated details and justification.</param>
        /// <returns>The updated service request.</returns>
        /// <response code="200">Request reopened successfully.</response>
        /// <response code="400">Invalid request data or unable to reopen.</response>
        /// <response code="401">Authentication is required.</response>
        /// <response code="403">User is not authorized to modify this request.</response>
        /// <response code="404">Service request not found.</response>
        [ProducesResponseType(typeof(ServiceRequestResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        [HttpPut("reopen")]
        public async Task<IActionResult> ReOpenRequest(
    [FromBody] ReopenServiceRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(request.Details) ||
                string.IsNullOrWhiteSpace(request.Justification))
            {
                return BadRequest(
                    "Details and justification cannot be empty.");
            }

            var existingRequest =
                _repository.GetRequestById(request.RequestId);

            if (existingRequest == null)
                return NotFound("Request not found.");

            // Only the original request owner can reopen.
            if (!string.Equals(
                existingRequest.RaisedBy,
                User.Identity?.Name,
                StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            // Only Resolved requests can be reopened.
            if (existingRequest.ReqStatus != 2)
            {
                return BadRequest(
                    "Only resolved requests can be reopened.");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // A request can be reopened only once.
                bool alreadyReopened =
                    await _auditLogService.HasBeenReopenedAsync(
                        request.RequestId);

                if (alreadyReopened)
                {
                    await transaction.RollbackAsync();

                    return BadRequest(
                        "This request has already been reopened once. " +
                        "Further reopening is not allowed.");
                }

                existingRequest.Details = request.Details.Trim();
                existingRequest.Justification =
                    request.Justification.Trim();

                bool result =
                    _repository.ReOpenRequest(existingRequest);

                if (!result)
                {
                    await transaction.RollbackAsync();

                    return BadRequest("Unable to reopen request.");
                }

                await _auditLogService.LogAsync(
                    request.RequestId,
                    "Reopened",
                    User.Identity?.Name ?? "Unknown",
                    "Service request reopened by its original owner."
                );

                await transaction.CommitAsync();

                var updatedRequest =
                    _repository.GetRequestById(request.RequestId);

                if (updatedRequest == null)
                    return NotFound(
                        "Updated request could not be retrieved.");

                var response = new ServiceRequestResponseDto
                {
                    RequestId = updatedRequest.RequestId,
                    Description = updatedRequest.Description,
                    Details = updatedRequest.Details,
                    RaisedBy = updatedRequest.RaisedBy,
                    RaisedOn = updatedRequest.RaisedOn,
                    Justification = updatedRequest.Justification,
                    ReqStatus = updatedRequest.ReqStatus
                };

                return Ok(response);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        // ==========================================
        // CREATE NEW SERVICE REQUEST
        // ==========================================
        /// <summary>
        /// Creates a new service request for the authenticated user.
        /// </summary>
        /// <remarks>
        /// The request owner is obtained from the JWT token.
        /// New requests are assigned the initial status.
        /// Creation is recorded in the audit log.
        /// </remarks>
        /// <param name="request">Description, details and justification.</param>
        /// <returns>The newly created service request.</returns>
        /// <response code="201">Service request created successfully.</response>
        /// <response code="400">Invalid request data.</response>
        /// <response code="401">Authentication is required.</response>
        [ProducesResponseType(typeof(ServiceRequestResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost("CreateNewSerRequest")]
        public async Task<IActionResult> CreateNewSerRequest(
            [FromBody] CreateServiceRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(request.Description) ||
                string.IsNullOrWhiteSpace(request.Details) ||
                string.IsNullOrWhiteSpace(request.Justification))
            {
                return BadRequest(
                    "Description, details and justification are required."
                );
            }

            string? loggedInUser = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(loggedInUser))
                return Unauthorized();

            var newRequest = new ServiceRequest
            {
                Description = request.Description.Trim(),
                Details = request.Details.Trim(),
                Justification = request.Justification.Trim(),
                RaisedBy = loggedInUser,
                ReqStatus = 1,
                RaisedOn = DateTime.Now
            };

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                int result = _repository.RaiseRequest(newRequest);

                if (result <= 0)
                    return BadRequest("Unable to create service request.");

                await _auditLogService.LogAsync(
                    result,
                    "Created",
                    loggedInUser,
                    "New service request created."
                );

                await transaction.CommitAsync();

                var response = new ServiceRequestResponseDto
                {
                    RequestId = result,
                    Description = newRequest.Description,
                    Details = newRequest.Details,
                    RaisedBy = newRequest.RaisedBy,
                    RaisedOn = newRequest.RaisedOn,
                    Justification = newRequest.Justification,
                    ReqStatus = newRequest.ReqStatus
                };

                return CreatedAtAction(
                    nameof(GetRequestById),
                    new { reqId = result },
                    response
                );
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ==========================================
        // GET REQUEST BY ID
        // ==========================================
        /// <summary>
        /// Retrieves a service request using its unique request ID.
        /// </summary>
        /// <remarks>
        /// Users can access their own requests. Administrators can access any request.
        /// </remarks>
        /// <param name="reqId">Unique service request ID.</param>
        /// <returns>The requested service request.</returns>
        /// <response code="200">Request retrieved successfully.</response>
        /// <response code="400">Invalid request ID.</response>
        /// <response code="401">Authentication is required.</response>
        /// <response code="403">User is not authorized to view this request.</response>
        /// <response code="404">Service request not found.</response>
        [ProducesResponseType(typeof(ServiceRequestResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("GetRequestById")]
        public IActionResult GetRequestById(int reqId)
        {
            if (reqId <= 0)
                return BadRequest("Valid Request ID is required.");

            var request = _repository.GetRequestById(reqId);

            if (request == null)
                return NotFound("Request not found.");

            // Admin can view any request.
            // Normal users can view only their own requests.
            if (!User.IsInRole("Admin") &&
                !string.Equals(
                    request.RaisedBy,
                    User.Identity?.Name,
                    StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            var response = new ServiceRequestResponseDto
            {
                RequestId = request.RequestId,
                Description = request.Description,
                Details = request.Details,
                RaisedBy = request.RaisedBy,
                RaisedOn = request.RaisedOn,
                Justification = request.Justification,
                ReqStatus = request.ReqStatus
            };

            return Ok(response);
        }

        // ==========================================
        // GET AUDIT HISTORY - ADMIN ONLY
        // ==========================================
        /// <summary>
        /// Retrieves paginated audit history with optional filters.
        /// </summary>
        /// <remarks>
        /// Restricted to administrators. Results can be filtered by request ID,
        /// username, action and date range.
        /// </remarks>
        /// <param name="pageNumber">Page number, starting from 1.</param>
        /// <param name="pageSize">Number of records per page (1–100).</param>
        /// <param name="requestId">Optional service request ID.</param>
        /// <param name="userName">Optional username filter.</param>
        /// <param name="action">Optional action: Created, Closed, Reopened or Deleted.</param>
        /// <param name="fromDate">Optional start date.</param>
        /// <param name="toDate">Optional end date.</param>
        /// <returns>A paginated collection of audit records.</returns>
        /// <response code="200">Audit history retrieved successfully.</response>
        /// <response code="400">Invalid pagination or filter parameters.</response>
        /// <response code="401">Authentication is required.</response>
        /// <response code="403">Administrator role is required.</response>
        [ProducesResponseType(typeof(PagedResponseDto<AuditLogResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "Admin")]
        [HttpGet("AuditHistory")]
        public async Task<IActionResult> GetAuditHistory(
            int pageNumber = 1,
            int pageSize = 10,
            int? requestId = null,
            string? userName = null,
            string? action = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            if (pageNumber < 1)
                return BadRequest("Page number must be at least 1.");

            if (pageSize < 1 || pageSize > 100)
                return BadRequest("Page size must be between 1 and 100.");

            if (requestId.HasValue && requestId.Value <= 0)
                return BadRequest("Invalid request ID.");

            if (userName?.Length > 20)
                return BadRequest("Username cannot exceed 20 characters.");

            if (action?.Length > 30)
                return BadRequest("Action cannot exceed 30 characters.");

            if (fromDate.HasValue &&
                toDate.HasValue &&
                fromDate.Value.Date > toDate.Value.Date)
            {
                return BadRequest("From date cannot be after To date.");
            }

            userName = string.IsNullOrWhiteSpace(userName)
                ? null
                : userName.Trim();

            if (!string.IsNullOrWhiteSpace(action))
            {
                action = action.Trim();

                string[] allowedActions =
{
    "Created",
    "Resolved",
    "Closed", // Retained for older audit records.
    "Reopened",
    "Deleted"
};

                string? matchingAction = allowedActions.FirstOrDefault(
                    a => string.Equals(
                        a,
                        action,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

                if (matchingAction == null)
                    return BadRequest("Invalid audit action.");

                // Use the canonical action name for filtering.
                action = matchingAction;
            }
            else
            {
                action = null;
            }

            var result =
                await _auditLogService.GetAuditLogsPagedAsync(
                    pageNumber,
                    pageSize,
                    requestId,
                    userName,
                    action,
                    fromDate,
                    toDate
                );

            var response = new PagedResponseDto<AuditLogResponseDto>
            {
                Items = result.Items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = result.TotalCount,
                TotalPages = (int)Math.Ceiling(
                    result.TotalCount / (double)pageSize
                )
            };

            return Ok(response);
        }

        // ==========================================
        // GET AUDIT SUMMARY - ADMIN ONLY
        // ==========================================
        /// <summary>
        /// Retrieves a summary of service request audit activity.
        /// </summary>
        /// <remarks>
        /// Restricted to administrators.
        /// </remarks>
        /// <returns>Audit activity summary.</returns>
        /// <response code="200">Audit summary retrieved successfully.</response>
        /// <response code="401">Authentication is required.</response>
        /// <response code="403">Administrator role is required.</response>
        [ProducesResponseType(
    typeof(AuditSummaryResponseDto),
    StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "Admin")]
        [HttpGet("AuditSummary")]
        public async Task<IActionResult> GetAuditSummary()
        {
            var summary =
                await _auditLogService.GetAuditSummaryAsync();

            return Ok(summary);
        }

        // ==========================================
        // GET USER DETAILS
        // ==========================================
        /// <summary>
        /// Retrieves a user's profile information.
        /// </summary>
        /// <remarks>
        /// Regular users can access only their own profile.
        /// Administrators can retrieve profiles using a username.
        /// </remarks>
        /// <param name="userName">Username to retrieve. Optional for administrators.</param>
        /// <returns>User profile including username, role and account creation date.</returns>
        /// <response code="200">User profile retrieved successfully.</response>
        /// <response code="400">Username is missing or invalid.</response>
        /// <response code="401">Authentication is required.</response>
        /// <response code="404">User not found.</response>
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("GetUser")]
        public IActionResult GetUser(string? userName = null)
        {
            string? loggedInUser = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(loggedInUser))
                return Unauthorized();

            // Normal users can access only their own profile.
            if (!User.IsInRole("Admin"))
            {
                userName = loggedInUser;
            }

            if (string.IsNullOrWhiteSpace(userName))
                return BadRequest("Username is required.");

            userName = userName.Trim();

            if (userName.Length > 20)
                return BadRequest("Username cannot exceed 20 characters.");

            var user = _repository.GetUser(userName);

            if (user == null)
                return NotFound("User not found.");

            var response = new UserResponseDto
            {
                UserName = user.UserName,
                Role = user.Role?.RoleName ?? "Unknown",
                CreatedOn = user.CreatedOn
            };

            return Ok(response);
        }

        // ==========================================
        // CLOSE SERVICE REQUEST - ADMIN ONLY
        // ==========================================
        /// <summary>
        /// Closes an existing service request.
        /// </summary>
        /// <remarks>
        /// Restricted to administrators. The operation is recorded in the audit log.
        /// </remarks>
        /// <param name="requestId">Unique service request ID.</param>
        /// <returns>The updated service request.</returns>
        /// <response code="200">Request closed successfully.</response>
        /// <response code="400">Invalid request ID or unable to close request.</response>
        /// <response code="401">Authentication is required.</response>
        /// <response code="403">Administrator role is required.</response>
        /// <response code="404">Service request not found.</response>
        [ProducesResponseType(typeof(ServiceRequestResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin")]
        [HttpPut("CloseRequest/{requestId:int}")]
        public async Task<IActionResult> CloseRequest(int requestId)
        {
            if (requestId <= 0)
                return BadRequest("Valid Request ID is required.");

            var existingRequest = _repository.GetRequestById(requestId);

            if (existingRequest == null)
                return NotFound("Request not found.");

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                bool result = _repository.CloseRequest(requestId);

                if (!result)
                    return BadRequest("Unable to close request.");

                await _auditLogService.LogAsync(
                    requestId,
                    "Resolved",
User.Identity?.Name ?? "Unknown",
"Service request resolved by administrator."
                );

                await transaction.CommitAsync();

                var updatedRequest = _repository.GetRequestById(requestId);

                if (updatedRequest == null)
                    return NotFound("Updated request could not be retrieved.");

                var response = new ServiceRequestResponseDto
                {
                    RequestId = updatedRequest.RequestId,
                    Description = updatedRequest.Description,
                    Details = updatedRequest.Details,
                    RaisedBy = updatedRequest.RaisedBy,
                    RaisedOn = updatedRequest.RaisedOn,
                    Justification = updatedRequest.Justification,
                    ReqStatus = updatedRequest.ReqStatus
                };

                return Ok(response);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ==========================================
        // DELETE SERVICE REQUEST
        // ==========================================
        /// <summary>
        /// Deletes an existing service request.
        /// </summary>
        /// <remarks>
        /// Users can delete their own requests. Administrators can delete any request.
        /// The deletion is recorded in the audit log, preserving the original owner.
        /// </remarks>
        /// <param name="requestId">Unique service request ID.</param>
        /// <response code="204">Request deleted successfully.</response>
        /// <response code="400">Invalid request ID or unable to delete request.</response>
        /// <response code="401">Authentication is required.</response>
        /// <response code="403">User is not authorized to delete this request.</response>
        /// <response code="404">Service request not found.</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        [HttpDelete("Delete/{requestId:int}")]
        public async Task<IActionResult> Delete(int requestId)
        {
            if (requestId <= 0)
                return BadRequest("Valid Request ID is required.");

            var existingRequest =
                _repository.GetRequestById(requestId);

            if (existingRequest == null)
                return NotFound("Request not found.");

            // Users can delete only their own requests.
            // Admins can delete any request.
            if (!User.IsInRole("Admin") &&
                !string.Equals(
                    existingRequest.RaisedBy,
                    User.Identity?.Name,
                    StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            // Preserve owner before deleting the request.
            string originalOwner = existingRequest.RaisedBy;

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                bool result = _repository.DeleteRequest(requestId);

                if (!result)
                    return BadRequest("Unable to delete request.");

                await _auditLogService.LogAsync(
                    requestId,
                    "Deleted",
                    User.Identity?.Name ?? "Unknown",
                    $"Service request deleted. Original owner: {originalOwner}"
                );

                await transaction.CommitAsync();

                return NoContent();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
