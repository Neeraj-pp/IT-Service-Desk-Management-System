
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Capstone.API.Configuration;
using HelpDesk.DAL.Model;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Capstone.API.Services
{
    /// <summary>
    /// Provides functionality for generating JWT access tokens
    /// for authenticated users.
    /// </summary>
    public sealed class JwtTokenService
    {
        private readonly JwtOptions _options;

        /// <summary>
        /// Initializes the JWT token service using configured JWT options.
        /// </summary>
        /// <param name="options">JWT configuration settings.</param>
        public JwtTokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// Generates a signed JWT access token for the specified user.
        /// </summary>
        /// <param name="user">The authenticated user whose identity and role are included in the token.</param>
        /// <returns>
        /// A tuple containing the generated JWT token and its UTC expiration time.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the user's role information is unavailable.
        /// </exception>
        public (string Token, DateTime ExpiresAtUtc) CreateToken(User user)
        {
            if (user.Role == null ||
                string.IsNullOrWhiteSpace(user.Role.RoleName))
            {
                throw new InvalidOperationException(
                    "The user role is not available."
                );
            }

            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(_options.AccessTokenMinutes);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserName),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role.RoleName)
            };

            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_options.Key)
            );

            var credentials = new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: credentials
            );

            return (
                new JwtSecurityTokenHandler().WriteToken(token),
                expires
            );
        }
    }
}
