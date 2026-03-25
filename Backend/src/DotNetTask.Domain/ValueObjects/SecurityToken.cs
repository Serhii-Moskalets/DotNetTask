using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Enums;
using DotNetTask.Domain.Exceptions;

namespace DotNetTask.Domain.ValueObjects;

/// <summary>
/// Represents a security token value object used for account-related operations
/// such as email verification, password resets, and email changes.
/// </summary>
public sealed record SecurityToken
{

    private SecurityToken() { }

    private SecurityToken(
        string value,
        DateTime expiresAt,
        UserTokenType type,
        string? metadata)
    {
        this.Value = value.Trim();
        this.ExpiresAt = expiresAt;
        this.Type = type;
        this.Metadata = metadata;
    }

    /// <summary>
    /// Gets the unique token string value.
    /// </summary>
    public string Value { get; } = string.Empty;

    /// <summary>
    /// Gets the date and time when the token expires.
    /// </summary>
    public DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Gets the purpose/type of the current token.
    /// </summary>
    public UserTokenType Type { get; init; }

    /// <summary>
    /// Gets optional additional data, such as a pending email address.
    /// </summary>
    public string? Metadata { get; init; }

    /// <summary>
    /// Creates a new security token with a specified duration.
    /// </summary>
    /// <param name="value">The token string.</param>
    /// <param name="duration">How long the token should remain valid from now.</param>
    /// <param name="type">The intended use of the token.</param>
    /// <param name="currentTime">The current time.</param>
    /// <param name="metadata">Optional associated data.</param>
    /// <returns>A new <see cref="SecurityToken"/> instance.</returns>
    public static SecurityToken Create(string value, TimeSpan duration, UserTokenType type, DateTime currentTime, string? metadata = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new DomainException(TokenPolicy.InvalidOrMissingMessage);
        }

        if (duration <= TimeSpan.Zero)
        {
            throw new DomainException(TokenPolicy.PositiveDurationMessage);
        }

        return new SecurityToken(value, currentTime.Add(duration), type, metadata);
    }

    /// <summary>
    /// Validates the token against a provided value, type, and point in time.
    /// </summary>
    /// <param name="value">The token string to verify.</param>
    /// <param name="expectedType">The expected purpose of the token.</param>
    /// <param name="currentTime">The time against which to check expiration (usually UtcNow).</param>
    /// <returns>True if the token matches the value and type and has not expired; otherwise, false.</returns>
    public bool IsValid(string value, UserTokenType expectedType, DateTime currentTime)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return this.Type == expectedType &&
               this.Value == value &&
               this.ExpiresAt > currentTime;
    }
}
