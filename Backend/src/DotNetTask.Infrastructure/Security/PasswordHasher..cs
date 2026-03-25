using DotNetTask.Application.Abstractions.Interfaces.Security;

namespace DotNetTask.Infrastructure.Security;

/// <summary>
/// Implementation of <see cref="IPasswordHasher"/> using the BCrypt algorithm.
/// </summary>
/// <remarks>
/// This implementation uses the "Enhanced" BCrypt methods, which support passwords
/// longer than 72 characters by pre-hashing them with SHA-384.
/// </remarks>
public class PasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Hashes a password using the BCrypt algorithm with a default work factor.
    /// </summary>
    /// <param name="password">The plain-text password to hash.</param>
    /// <returns>A 60-character BCrypt hash string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="password"/> is null or empty.</exception>
    public string HashPassword(string password)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(password);
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password);
    }

    /// <summary>
    /// Verifies that the provided plain-text password matches the BCrypt hash.
    /// </summary>
    /// <param name="password">The plain-text password to verify.</param>
    /// <param name="hashedPassword">The BCrypt hash to compare against.</param>
    /// <returns><c>true</c> if the password is valid; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="password"/> or <paramref name="hashedPassword"/> is null or empty.
    /// </exception>
    public bool VerifyPassword(string password, string hashedPassword)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(password);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(hashedPassword);
        return BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword);
    }
}
