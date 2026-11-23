
namespace Capstone.API.DTOs
{
    /// <summary>
    /// Represents an individual audit log entry returned by the API.
    /// </summary>
    public class AuditLogResponseDto
    {
        /// <summary>
        /// Gets or sets the unique audit log ID.
        /// </summary>
        public int AuditId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the related service request.
        /// </summary>
        public int RequestId { get; set; }

        /// <summary>
        /// Gets or sets the action performed, such as Created, Closed,
        /// Reopened or Deleted.
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the username of the person who performed the action.
        /// </summary>
        public string PerformedBy { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the UTC date and time when the action was performed.
        /// </summary>
        public DateTime PerformedAt { get; set; }

        /// <summary>
        /// Gets or sets additional information about the audit event.
        /// </summary>
        public string? Details { get; set; }
    }
}
