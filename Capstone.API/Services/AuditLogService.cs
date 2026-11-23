
using Capstone.API.DTOs;
using HelpDesk.DAL.Model;
using Microsoft.EntityFrameworkCore;

namespace Capstone.API.Services
{
    /// <summary>
    /// Provides services for recording, retrieving and summarizing
    /// service request audit activities.
    /// </summary>
    public class AuditLogService
    {
        private readonly HelpDeskDbContext _context;

        /// <summary>
        /// Initializes the audit log service with the database context.
        /// </summary>
        /// <param name="context">The Entity Framework database context.</param>
        public AuditLogService(HelpDeskDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Records an activity performed on a service request.
        /// </summary>
        /// <param name="requestId">The ID of the affected service request.</param>
        /// <param name="action">The activity performed, such as Created, Closed, Reopened or Deleted.</param>
        /// <param name="performedBy">The username of the person who performed the activity.</param>
        /// <param name="details">Optional additional information about the activity.</param>
        /// <returns>A task representing the asynchronous logging operation.</returns>
        public async Task LogAsync(
            int requestId,
            string action,
            string performedBy,
            string? details = null)
        {
            var auditLog = new AuditLog
            {
                RequestId = requestId,
                Action = action,
                PerformedBy = performedBy,
                PerformedAt = DateTime.UtcNow,
                Details = details
            };

            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves audit history using optional filters and pagination.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="requestId">Optional service request ID filter.</param>
        /// <param name="userName">Optional username filter.</param>
        /// <param name="action">Optional activity type filter.</param>
        /// <param name="fromDate">Optional starting date for filtering activities.</param>
        /// <param name="toDate">Optional ending date for filtering activities.</param>
        /// <returns>
        /// A tuple containing the audit records for the requested page
        /// and the total number of matching records.
        /// </returns>
        public async Task<(List<AuditLogResponseDto> Items, int TotalCount)>
            GetAuditLogsPagedAsync(
                int pageNumber,
                int pageSize,
                int? requestId,
                string? userName,
                string? action,
                DateTime? fromDate,
                DateTime? toDate)
        {
            IQueryable<AuditLog> query =
                _context.AuditLogs.AsNoTracking();

            if (requestId.HasValue)
            {
                query = query.Where(a => a.RequestId == requestId.Value);
            }

            if (!string.IsNullOrWhiteSpace(userName))
            {
                string searchUser = userName.Trim();

                query = query.Where(a =>
                    a.PerformedBy.Contains(searchUser));
            }

            if (!string.IsNullOrWhiteSpace(action))
            {
                string searchAction = action.Trim();

                query = query.Where(a =>
                    a.Action == searchAction);
            }

            if (fromDate.HasValue)
            {
                DateTime startDate = fromDate.Value.Date;

                query = query.Where(a =>
                    a.PerformedAt >= startDate);
            }

            if (toDate.HasValue)
            {
                DateTime endDate = toDate.Value.Date.AddDays(1);

                query = query.Where(a =>
                    a.PerformedAt < endDate);
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(a => a.PerformedAt)
                .ThenByDescending(a => a.AuditId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuditLogResponseDto
                {
                    AuditId = a.AuditId,
                    RequestId = a.RequestId,
                    Action = a.Action,
                    PerformedBy = a.PerformedBy,
                    PerformedAt = a.PerformedAt,
                    Details = a.Details
                })
                .ToListAsync();

            return (items, totalCount);
        }

        /// <summary>
        /// Calculates summary statistics for recorded audit activities.
        /// </summary>
        /// <returns>
        /// An audit summary containing the total activities and counts
        /// for created, closed, reopened and deleted requests.
        /// </returns>
        public async Task<AuditSummaryResponseDto> GetAuditSummaryAsync()
        {
            var query = _context.AuditLogs.AsNoTracking();

            return new AuditSummaryResponseDto
            {
                TotalActivities = await query.CountAsync(),

                CreatedCount = await query.CountAsync(
                    a => a.Action == "Created"),

                ClosedCount = await query.CountAsync(
    a => a.Action == "Resolved" || a.Action == "Closed"),

                ReopenedCount = await query.CountAsync(
                    a => a.Action == "Reopened"),

                DeletedCount = await query.CountAsync(
                    a => a.Action == "Deleted")
            };
        }

        public async Task<bool> HasBeenReopenedAsync(int requestId)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .AnyAsync(a =>
                    a.RequestId == requestId &&
                    a.Action == "Reopened");
        }

    }
}
