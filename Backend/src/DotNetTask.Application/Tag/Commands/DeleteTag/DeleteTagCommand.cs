using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Tag.Commands.DeleteTag;

/// <summary>
/// Represents a command to delete a tag belonging to a specific user.
/// </summary>
public record DeleteTagCommand(Guid TagId, Guid UserId) : ICommand;
