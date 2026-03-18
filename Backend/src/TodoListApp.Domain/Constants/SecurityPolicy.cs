namespace TodoListApp.Domain.Constants;

/// <summary>
/// Provides constants for security-related policies and error messages.
/// </summary>
public static class SecurityPolicy
{
    /// <summary>
    /// Error message for an invalid or empty security stamp.
    /// </summary>
    public const string InvalidSecurityStampMessage = "Invalid security identifier.";

    /// <summary>Error message when the generated password hash is empty.</summary>
    public const string HashEmptyMessage = "Password hash cannot be empty.";
}
