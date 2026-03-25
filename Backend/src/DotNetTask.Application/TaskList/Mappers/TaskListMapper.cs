using DotNetTask.Application.TaskList.Dtos;
using DotNetTask.Domain.Entities;

using Riok.Mapperly.Abstractions;

namespace DotNetTask.Application.TaskList.Mappers;

/// <summary>
/// Provides mapping methods to convert <see cref="TaskListEntity"/> objects
/// to <see cref="TaskListDto"/> objects using Mapperly.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class TaskListMapper
{
    /// <summary>
    /// Maps a single <see cref="TaskListEntity"/> to a <see cref="TaskListDto"/>.
    /// </summary>
    /// <param name="entity">The source task list entity.</param>
    /// <returns>A <see cref="TaskListDto"/> with mapped properties.</returns>
    public static partial TaskListDto Map(TaskListEntity entity);

    /// <summary>
    /// Maps a collection of <see cref="TaskListEntity"/> objects to a list of <see cref="TaskListDto"/> objects.
    /// </summary>
    /// <param name="entities">The source collection of task list entities.</param>
    /// <returns>A list of <see cref="TaskListDto"/> objects.</returns>
    public static partial IReadOnlyCollection<TaskListDto> Map(IReadOnlyCollection<TaskListEntity> entities);
}
