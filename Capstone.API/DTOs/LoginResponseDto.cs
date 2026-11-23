
namespace Capstone.API.DTOs
{
    /// <summary>
    /// Represents the response returned after successful authentication.
    /// </summary>
    public sealed class LoginResponseDto
    {
        /// <summary>
        /// Gets or sets the JWT access token used to authorize API requests.
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the authentication scheme, normally Bearer.
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// Gets or sets the UTC date and time when the token expires.
        /// </summary>
        public DateTime ExpiresAtUtc { get; set; }

        /// <summary>
        /// Gets or sets the authenticated user's basic profile information.
        /// </summary>
        public LoginUserDto User { get; set; } = new();
    }

    /// <summary>
    /// Represents the basic user information included in the login response.
    /// </summary>
    public class LoginUserDto
    {
        /// <summary>
        /// Gets or sets the authenticated username.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the authenticated user's role.
        /// </summary>
        public string Role { get; set; } = string.Empty;
    }
}
