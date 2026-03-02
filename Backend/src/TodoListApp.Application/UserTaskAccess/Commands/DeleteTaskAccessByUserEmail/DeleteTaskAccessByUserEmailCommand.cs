using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessByUserEmail;

/// <summary>
/// Command to delete a user-task access entry based on the task ID and the user's email.
/// </summary>
/// <param name="TaskId">The unique identifier of the task.</param>
/// <param name="OwnerId">The ID of the task owner performing the action.</param>
/// <param name="Email">The email of the user whose access is being revoked.</param>
public record DeleteTaskAccessByUserEmailCommand(Guid TaskId, Guid OwnerId, string Email)
    : ICommand;
