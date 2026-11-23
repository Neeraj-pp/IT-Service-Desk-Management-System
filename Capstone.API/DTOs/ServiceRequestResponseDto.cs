
namespace Capstone.API.DTOs
{
    /// <summary>
    /// Represents the service request information returned by the API.
    /// </summary>
    public class ServiceRequestResponseDto
    {
        /// <summary>
        /// Gets or sets the unique service request ID.
        /// </summary>
        public int RequestId { get; set; }

        /// <summary>
        /// Gets or sets the short description of the request.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the detailed explanation of the issue.
        /// </summary>
        public string Details { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the username of the person who raised the request.
        /// </summary>
        public string RaisedBy { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date and time when the request was raised.
        /// </summary>
        public DateTime RaisedOn { get; set; }

        /// <summary>
        /// Gets or sets the justification provided for the request.
        /// </summary>
        public string Justification { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current service request status ID.
        /// </summary>
        public int ReqStatus { get; set; }
    }
}
