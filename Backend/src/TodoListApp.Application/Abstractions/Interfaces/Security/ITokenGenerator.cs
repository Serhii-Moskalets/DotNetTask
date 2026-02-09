namespace TodoListApp.Application.Abstractions.Interfaces.Security;

/// <summary>
/// Defines a contract for generating cryptographically secure tokens
/// used for security-sensitive operations such as email verification,
/// password resets, and account changes.
/// </summary>
public interface ITokenGenerator
{
    /// <summary>
    /// Generates a high-entropy, cryptographically secure random string.
    /// </summary>
    /// <returns>
    /// A string representing a unique security token,
    /// typically formatted as a hexadecimal or base64 string.
    /// </returns>
    /// <remarks>
    /// The generated token is suitable for use in URLs and should be
    /// practically impossible to guess or brute-force.
    /// </remarks>
    string GenerateSecureToken();
}
