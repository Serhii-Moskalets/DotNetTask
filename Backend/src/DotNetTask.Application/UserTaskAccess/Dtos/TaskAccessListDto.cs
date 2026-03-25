using DotNetTask.Application.Common.Dtos;

namespace DotNetTask.Application.UserTaskAccess.Dtos;

/// <summary>
/// Represents a task along with the list of users who have access to it.
/// Used by the task owner to see who can access their tasks.
/// </summary>
public class TaskAccessListDto
{
    /// <summary>
    /// Gets the unique identifier of the task.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the title of the task.
    /// </summary>
    public string Title { get; init; } = null!;

    /// <summary>
    /// Gets the collection of users who have access to this task.
    /// </summary>
    public PagedResultDto<UserBriefDto> Users { get; init; } = null!;
}
