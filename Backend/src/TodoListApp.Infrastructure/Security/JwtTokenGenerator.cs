using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TodoListApp.Application.Abstractions.Interfaces.Security;
using TodoListApp.Domain.Entities;
using TodoListApp.Infrastructure.Test.Security.Settings;

namespace TodoListApp.Infrastructure.Security;

/// <summary>
/// Generates JSON Web Tokens (JWT) for authenticated users.
/// </summary>
/// <remarks>
/// This implementation creates a signed JWT containing standard claims
/// such as user identifier, email, username, and a unique token identifier.
/// The token configuration is provided via <see cref="JwtSettings"/>.
/// </remarks>
public class JwtTokenGenerator(IOptions<JwtSettings> jwtOptions) : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    /// <summary>
    /// Generates a signed JSON Web Token (JWT) for the specified user.
    /// </summary>
    /// <param name="user">
    /// The user entity for which the authentication token is generated.
    /// </param>
    /// <returns>
    /// A string representation of the generated JWT.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the <paramref name="user"/> argument is <c>null</c>.
    /// </exception>
    public string GenerateToken(UserEntity user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new (JwtRegisteredClaimNames.Email, user.Email.Value),
            new (JwtRegisteredClaimNames.UniqueName, user.UserName.Value),
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new ("security_stamp", user.SecurityStamp.Value),
        };

        var token = new JwtSecurityToken(
            issuer: this._jwtSettings.Issuer,
            audience: this._jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(this._jwtSettings.ExpiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
