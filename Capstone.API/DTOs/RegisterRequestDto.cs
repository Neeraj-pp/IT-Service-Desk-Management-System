
using System.ComponentModel.DataAnnotations;

namespace Capstone.API.DTOs
{
    /// <summary>
    /// Represents the data required to register a new user account.
    /// </summary>
    public class RegisterRequestDto
    {
        /// <summary>
        /// Gets or sets the username. Must contain 3–20 letters,
        /// numbers or underscores.
        /// </summary>
        [Required]
        [RegularExpression(
            @"^[a-zA-Z0-9_]{3,20}$",
            ErrorMessage = "Username must be 3-20 characters and contain only letters, numbers or underscores.")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the account password. Must be 8–64 characters.
        /// </summary>
        [Required]
        [StringLength(64, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password confirmation.
        /// Must match the Password property.
        /// </summary>
        [Required]
        [Compare(nameof(Password),
            ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
