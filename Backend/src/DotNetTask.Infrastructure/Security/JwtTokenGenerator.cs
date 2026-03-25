using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotNetTask.Application.Abstractions.Interfaces.Security;
using DotNetTask.Domain.Entities;
using DotNetTask.Infrastructure.Security.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DotNetTask.Infrastructure.Security;

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

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(this._jwtSettings.Secret));
        SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new (JwtRegisteredClaimNames.Email, user.Email.Value),
            new (JwtRegisteredClaimNames.UniqueName, user.UserName.Value),
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (CustomClaims.SecurityStamp, user.SecurityStamp.Value),
        ];

        JwtSecurityToken token = new(
            issuer: this._jwtSettings.Issuer,
            audience: this._jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(this._jwtSettings.ExpiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
