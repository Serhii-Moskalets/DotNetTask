namespace DotNetTask.Api.Requests.Auth;

/// <summary>
/// Represents a request to register a new user in the system.
/// </summary>
/// <param name="FirstName">The user's first name.</param>
/// <param name="UserName">The unique username chosen by the user.</param>
/// <param name="Email">The unique email address of the user used for communication and security.</param>
/// <param name="Password">The plain-text password provided by the user, to be hashed before storage.</param>
/// <param name="LastName">The user's last name (optional).</param>
public record RegisterRequest(
    string FirstName,
    string UserName,
    string Email,
    string Password,
    string? LastName = null);
