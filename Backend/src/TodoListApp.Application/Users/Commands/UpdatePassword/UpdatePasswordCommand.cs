using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Users.Commands.UpdatePassword;

/// <summary>
/// Command to update the password for the current user.
/// </summary>
/// <param name="CurrentPassword">The user's current password for identity verification.</param>
/// <param name="NewPassword">The new password to be set.</param>
/// <param name="UserId">The unique identifier of the user whose password is being updated.</param>
public record UpdatePasswordCommand(
    string CurrentPassword,
    string NewPassword,
    Guid UserId) : ICommand<bool>;
