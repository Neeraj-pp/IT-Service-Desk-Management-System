
namespace Capstone.API.DTOs
{
    /// <summary>
    /// Represents the response returned after successful registration.
    /// </summary>
    public class RegisterResponseDto
    {
        /// <summary>
        /// Gets or sets the registration result message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the registered username.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the role assigned to the new account.
        /// </summary>
        public string Role { get; set; } = string.Empty;
    }
}
