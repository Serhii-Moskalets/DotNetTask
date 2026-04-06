using DotNetTask.Application.Common.Dtos;
using DotNetTask.Domain.Entities;

using Riok.Mapperly.Abstractions;

namespace DotNetTask.Application.UserTaskAccess.Mappers;

/// <summary>
/// Provides mapping methods for converting user task access–related domain entities
/// into DTOs used when a shared task is viewed by a non-owner user.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class TaskAccessForUserMapper
{
    /// <summary>
    /// Maps a <see cref="UserEntity"/> to a <see cref="UserBriefDto"/>.
    /// </summary>
    /// <param name="user">The user entity.</param>
    /// <returns>A <see cref="UserBriefDto"/>.</returns>
    public static partial UserBriefDto Map(UserEntity user);

    /// <summary>
    /// Maps a <see cref="TagEntity"/> to a <see cref="TagDto"/>.
    /// </summary>
    /// <param name="tag">The tag entity.</param>
    /// <returns>A <see cref="TagDto"/>.</returns>
    public static partial TagDto Map(TagEntity tag);

    /// <summary>
    /// Maps a <see cref="TaskEntity"/> to a <see cref="TaskDto"/>.
    /// </summary>
    /// <param name="task">The task entity.</param>
    /// <returns>A <see cref="TaskDto"/>.</returns>
    public static partial TaskDto Map(TaskEntity task);

    /// <summary>
    /// Maps a <see cref="UserTaskAccessEntity"/> to a <see cref="TaskDto"/>
    /// using the associated task entity.
    /// </summary>
    /// <param name="uta">The user-task access entity.</param>
    /// <returns>A <see cref="TaskDto"/> representing the shared task.</returns>
    public static TaskDto Map(UserTaskAccessEntity uta) => Map(uta.Task);

    /// <summary>
    /// Maps a collection of <see cref="UserTaskAccessEntity"/> instances
    /// to a list of <see cref="TaskDto"/> objects.
    /// </summary>
    /// <param name="utaList">The collection of user-task access entities.</param>
    /// <returns>A list of <see cref="TaskDto"/> objects.</returns>
    public static IReadOnlyCollection<TaskDto> Map(IReadOnlyCollection<UserTaskAccessEntity> utaList) => utaList.Select(Map).ToList();
}
