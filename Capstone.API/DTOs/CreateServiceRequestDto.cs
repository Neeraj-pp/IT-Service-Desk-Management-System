
using System.ComponentModel.DataAnnotations;

namespace Capstone.API.DTOs
{
    /// <summary>
    /// Represents the information required to create a service request.
    /// </summary>
    public class CreateServiceRequestDto
    {
        /// <summary>
        /// Gets or sets a short description of the service request.
        /// Must be 3–50 characters.
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the detailed explanation of the issue.
        /// Must be 5–100 characters.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string Details { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the justification for raising the request.
        /// Must be 3–50 characters.
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Justification { get; set; } = string.Empty;
    }
}
