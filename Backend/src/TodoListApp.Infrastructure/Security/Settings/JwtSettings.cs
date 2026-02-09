namespace TodoListApp.Infrastructure.Test.Security.Settings;

/// <summary>
/// Represents configuration settings used for JSON Web Token (JWT) generation.
/// </summary>
/// <remarks>
/// These settings are typically loaded from application configuration
/// and bound to the <see cref="SectionName"/> section.
/// </remarks>
public class JwtSettings
{
    /// <summary>
    /// The configuration section name used to bind JWT settings.
    /// </summary>
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// Gets the secret key used to sign JWT tokens.
    /// </summary>
    public string Secret { get; init; } = null!;

    /// <summary>
    /// Gets the token expiration time in minutes.
    /// </summary>
    public int ExpiryMinutes { get; init; }

    /// <summary>
    /// Gets the token issuer identifier.
    /// </summary>
    public string Issuer { get; init; } = null!;

    /// <summary>
    /// Gets the intended audience for the JWT.
    /// </summary>
    public string Audience { get; init; } = null!;
}
