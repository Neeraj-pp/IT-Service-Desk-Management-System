
using System.ComponentModel.DataAnnotations;

namespace Capstone.API.DTOs
{
    /// <summary>
    /// Represents the credentials submitted during user login.
    /// </summary>
    public sealed class LoginRequestDto
    {
        /// <summary>
        /// Gets or sets the username. Must be 3–20 characters.
        /// </summary>
        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password. Must be 8–64 characters.
        /// </summary>
        [Required]
        [StringLength(64, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;
    }
}
