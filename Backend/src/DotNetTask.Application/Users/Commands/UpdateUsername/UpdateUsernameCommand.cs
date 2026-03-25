using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Users.Commands.UpdateUsername;

/// <summary>
/// Command to update an username for current user.
/// </summary>
/// <param name="UserName">The current user username.</param>
/// <param name="UserId">The current user id</param>
public record UpdateUsernameCommand(
    string UserName,
    Guid UserId) : ICommand<bool>;
