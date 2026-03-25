using DotNetTask.Application.Common.Dtos;
using DotNetTask.Domain.Entities;

using Riok.Mapperly.Abstractions;

namespace DotNetTask.Application.UserTaskAccess.Mappers;

/// <summary>
/// Provides mapping methods for converting <see cref="UserTaskAccessEntity"/> instances
/// to Data Transfer Objects (DTOs) used for task access management.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class TaskAccessForOwnerMapper
{
    /// <summary>
    /// Maps a single <see cref="UserTaskAccessEntity"/> to a <see cref="UserBriefDto"/>.
    /// </summary>
    /// <param name="entity">The user-task access entity to map.</param>
    /// <returns>A <see cref="UserBriefDto"/> representing the user's access information.</returns>
    [MapProperty(nameof(UserTaskAccessEntity.User.Id), nameof(UserBriefDto.Id))]
    [MapProperty(nameof(UserTaskAccessEntity.User.FirstName), nameof(UserBriefDto.FirstName))]
    [MapProperty(nameof(UserTaskAccessEntity.User.LastName), nameof(UserBriefDto.LastName))]
    [MapProperty(nameof(UserTaskAccessEntity.User.UserName), nameof(UserBriefDto.UserName))]
    [MapProperty(nameof(UserTaskAccessEntity.User.Email), nameof(UserBriefDto.Email))]
    [MapperIgnoreSource(nameof(UserTaskAccessEntity.Task))]
    [MapperIgnoreSource(nameof(UserTaskAccessEntity.TaskId))]
    [MapperIgnoreSource(nameof(UserTaskAccessEntity.UserId))]
    public static partial UserBriefDto Map(UserTaskAccessEntity entity);

    /// <summary>
    /// Maps a collection of <see cref="UserTaskAccessEntity"/> instances
    /// to a list of <see cref="UserBriefDto"/> objects.
    /// </summary>
    /// <param name="entities">The collection of user-task access entities.</param>
    /// <returns>A list of <see cref="UserBriefDto"/> objects.</returns>
    public static partial IReadOnlyCollection<UserBriefDto> Map(IReadOnlyCollection<UserTaskAccessEntity> entities);
}
