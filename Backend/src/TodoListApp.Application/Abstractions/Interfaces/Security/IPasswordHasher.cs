namespace TodoListApp.Application.Abstractions.Interfaces.Security;

/// <summary>
/// Provides methods for securely hashing and verifying passwords.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes the specified plain-text password.
    /// </summary>
    /// <param name="password">The plain-text password to hash.</param>
    /// <returns>A secure, salted, and hashed representation of the password.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the password is null or empty.</exception>
    string HashPassword(string password);

    /// <summary>
    /// Verifies that a plain-text password matches a previously generated hash.
    /// </summary>
    /// <param name="password">The plain-text password to verify.</param>
    /// <param name="hashedPassword">The stored hash to compare against.</param>
    /// <returns>
    /// <c>true</c> if the password matches the hash; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method is resistant to timing attacks.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="password"/> or <paramref name="hashedPassword"/> is null or empty.
    /// </exception>
    bool VerifyPassword(string password, string hashedPassword);
}
