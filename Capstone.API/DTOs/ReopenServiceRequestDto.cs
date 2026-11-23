
using System.ComponentModel.DataAnnotations;

namespace Capstone.API.DTOs
{
    /// <summary>
    /// Represents the data required to reopen an existing service request.
    /// </summary>
    public class ReopenServiceRequestDto
    {
        /// <summary>
        /// Gets or sets the ID of the service request to reopen.
        /// Must be a positive integer.
        /// </summary>
        [Range(1, int.MaxValue,
            ErrorMessage = "Valid Request ID is required.")]
        public int RequestId { get; set; }

        /// <summary>
        /// Gets or sets the updated details of the service request.
        /// Must be 5–100 characters.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string Details { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the justification for reopening the request.
        /// Must be 3–50 characters.
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Justification { get; set; } = string.Empty;
    }
}
