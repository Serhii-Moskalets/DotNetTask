namespace TodoListApp.Application.Abstractions.Interfaces.Security;

/// <summary>
/// Provides a centralized definition of custom JWT claim types used throughout the application.
/// </summary>
/// <remarks>
/// These constants ensure consistency between token generation in the Infrastructure layer
/// and claim validation in the Middleware or Authorization layers.
/// </remarks>
public static class CustomClaims
{
    /// <summary>
    /// The claim type used to store the user's security stamp.
    /// Used for session invalidation and security verification.
    /// </summary>
    public const string SecurityStamp = "security_stamp";
}
