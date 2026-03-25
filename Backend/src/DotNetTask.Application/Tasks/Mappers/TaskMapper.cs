using DotNetTask.Application.Common.Dtos;
using DotNetTask.Domain.Entities;

using Riok.Mapperly.Abstractions;

namespace DotNetTask.Application.Tasks.Mappers;

/// <summary>
/// Provides mapping methods to convert domain entities to their corresponding DTOs.
/// Uses Mapperly with <see cref="RequiredMappingStrategy.Target"/> to enforce mapping completeness.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class TaskMapper
{
    /// <summary>
    /// Maps a <see cref="TaskEntity"/> to a <see cref="TaskDto"/>.
    /// </summary>
    /// <param name="entity">The task entity to map.</param>
    /// <returns>The mapped <see cref="TaskDto"/>.</returns>
    public static partial TaskDto Map(TaskEntity entity);

    /// <summary>
    /// Maps a <see cref="TagEntity"/> to a <see cref="TagDto"/>.
    /// </summary>
    /// <param name="entity">The tag entity to map.</param>
    /// <returns>The mapped <see cref="TagDto"/>.</returns>
    public static partial TagDto Map(TagEntity entity);

    /// <summary>
    /// Maps a <see cref="CommentEntity"/> to a <see cref="CommentDto"/>.
    /// </summary>
    /// <param name="entity">The comment entity to map.</param>
    /// <returns>The mapped <see cref="CommentDto"/>.</returns>
    public static partial CommentDto Map(CommentEntity entity);

    /// <summary>
    /// Maps a <see cref="UserEntity"/> to a <see cref="UserBriefDto"/>.
    /// </summary>
    /// <param name="entity">The user entity to map.</param>
    /// <returns>The mapped <see cref="UserBriefDto"/>.</returns>
    public static partial UserBriefDto Map(UserEntity entity);

    /// <summary>
    /// Maps a collection of <see cref="TaskEntity"/> objects to a list of <see cref="TaskDto"/> objects.
    /// </summary>
    /// <param name="entities">The collection of task entities to map.</param>
    /// <returns>A list of mapped <see cref="TaskDto"/> objects.</returns>
    public static partial IReadOnlyCollection<TaskDto> Map(IReadOnlyCollection<TaskEntity> entities);

    /// <summary>
    /// Maps a <see cref="TaskEntity"/> to a summary <see cref="TaskBriefDto"/>.
    /// </summary>
    /// <param name="entity">The task entity to map.</param>
    /// <returns>The mapped <see cref="TaskBriefDto"/>.</returns>
    public static partial TaskBriefDto MapToBrief(TaskEntity entity);

    /// <summary>
    /// Maps a collection of <see cref="TaskEntity"/> objects to a list of <see cref="TaskBriefDto"/> objects.
    /// </summary>
    /// <param name="entities">The collection of task entities to map.</param>
    /// <returns>A list of mapped <see cref="TaskBriefDto"/> objects.</returns>
    public static partial IReadOnlyCollection<TaskBriefDto> MapToBrief(IReadOnlyCollection<TaskEntity> entities);
}
