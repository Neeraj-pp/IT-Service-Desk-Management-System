
namespace Capstone.API.DTOs
{
    /// <summary>
    /// Represents the user profile information returned by the API.
    /// </summary>
    public class UserResponseDto
    {
        /// <summary>
        /// Gets or sets the user's username.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's assigned role.
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date and time when the account was created.
        /// </summary>
        public DateTime CreatedOn { get; set; }
    }
}
