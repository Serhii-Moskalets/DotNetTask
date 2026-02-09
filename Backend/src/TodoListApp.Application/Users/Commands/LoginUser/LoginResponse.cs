namespace TodoListApp.Application.Users.Commands.LoginUser;

/// <summary>
/// Represents the response returned after a successful user authentication.
/// </summary>
/// <param name="Id">
/// The unique identifier of the authenticated user.
/// </param>
/// <param name="UserName">
/// The username of the authenticated user.
/// </param>
/// <param name="Email">
/// The email address of the authenticated user.
/// </param>
/// <param name="Teken">
/// The authentication token issued for the user session.
/// </param>
public record LoginResponse(
    Guid Id,
    string UserName,
    string Email,
    string Teken);
