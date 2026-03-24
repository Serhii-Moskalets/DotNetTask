namespace TodoListApp.Application.Users.Commands.LoginUser;

/// <summary>
/// Represents the data returned after a successful login.
/// </summary>
/// <param name="Id">The unique identifier of the user.</param>
/// <param name="UserName">The user's unique username.</param>
/// <param name="Email">The user's registered email address.</param>
/// <param name="Token">The generated JWT for subsequent authenticated requests.</param>
/// <param name="IsEmailConfirmed">A flag indicating whether the user's email address has been confirmed.</param>
/// <param name="MustChangePassword">A flag indicating whether the user is required to change their password before they can access the application's full functionality.</param>
public record LoginResponse(
    Guid Id,
    string UserName,
    string Email,
    string? Token,
    bool IsEmailConfirmed = true,
    bool MustChangePassword = false);
