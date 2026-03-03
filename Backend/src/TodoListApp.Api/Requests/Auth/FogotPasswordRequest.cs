namespace TodoListApp.Api.Requests.Auth;

/// <summary>
/// Represents a request to initiate the password reset process.
/// </summary>
/// <param name="Email">The email address associated with the user account.</param>
public record ForgotPasswordRequest(string Email);
