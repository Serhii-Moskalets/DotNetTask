using Riok.Mapperly.Abstractions;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Users.Mappers;

/// <summary>
/// Provides mapping methods to convert domain entities to their corresponding DTOs.
/// Uses Mapperly with <see cref="RequiredMappingStrategy.Target"/> to enforce mapping completeness.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class UserMapper
{
    /// <summary>
    /// Maps a <see cref="UserEntity"/> to a <see cref="UserBriefDto"/>.
    /// </summary>
    /// <param name="entity">The user entity to map</param>
    /// <returns>The mapped <see cref="UserBriefDto"/>.</returns>
    public static partial UserBriefDto Map(UserEntity entity);
}
