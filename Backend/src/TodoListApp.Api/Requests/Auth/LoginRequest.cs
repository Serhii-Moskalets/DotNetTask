namespace TodoListApp.Api.Requests.Auth;

/// <summary>
/// Represents a request to authenticate a user.
/// </summary>
/// <param name="Email">The user's registered email address.</param>
/// <param name="Password">The user's password.</param>
public record LoginRequest(
    string Email,
    string Password);
