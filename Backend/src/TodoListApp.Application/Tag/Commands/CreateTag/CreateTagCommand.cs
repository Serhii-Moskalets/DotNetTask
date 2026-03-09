using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Tag.Commands.CreateTag;

/// <summary>
/// Command for creating a new tag for a specific task and user.
/// </summary>
/// <param name="UserId">The unique identifier of the user who owns the tag.</param>
/// <param name="TaskId">The unique identifier of the task to which the tag will be attached.</param>
/// <param name="Name">
/// The raw name of the tag.
/// This value will be validated and converted into a <see cref="Domain.ValueObjects.TagName"/> object.
/// </param>
public record CreateTagCommand(
    Guid UserId,
    Guid TaskId,
    string Name) : ICommand<Guid>;
