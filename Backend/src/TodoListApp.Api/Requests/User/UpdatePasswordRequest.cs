namespace TodoListApp.Api.Requests.User;

/// <summary>
/// Represents a request to update the password of an authenticated user.
/// </summary>
/// <param name="CurrentPassword">The user's current password for verification.</param>
/// <param name="NewPassword">The new password to be set.</param>
public record UpdatePasswordRequest(
    string CurrentPassword,
    string NewPassword);