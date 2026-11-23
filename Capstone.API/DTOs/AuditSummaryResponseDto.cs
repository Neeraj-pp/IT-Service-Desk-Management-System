
namespace Capstone.API.DTOs
{
    /// <summary>
    /// Represents a summary of recorded service request activities.
    /// </summary>
    public class AuditSummaryResponseDto
    {
        /// <summary>
        /// Gets or sets the total number of recorded activities.
        /// </summary>
        public int TotalActivities { get; set; }

        /// <summary>
        /// Gets or sets the total number of service requests created.
        /// </summary>
        public int CreatedCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of service requests closed.
        /// </summary>
        public int ClosedCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of service requests reopened.
        /// </summary>
        public int ReopenedCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of service requests deleted.
        /// </summary>
        public int DeletedCount { get; set; }
    }
}
