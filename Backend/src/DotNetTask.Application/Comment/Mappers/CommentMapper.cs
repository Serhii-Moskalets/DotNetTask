using DotNetTask.Application.Common.Dtos;
using DotNetTask.Domain.Entities;

using Riok.Mapperly.Abstractions;

namespace DotNetTask.Application.Comment.Mappers;

/// <summary>
/// Provides mapping methods between domain entities and Data Transfer Objects (DTOs) for comments and users.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class CommentMapper
{
    /// <summary>
    /// Maps a <see cref="CommentEntity"/> to a <see cref="CommentDto"/>.
    /// </summary>
    /// <param name="entity">The comment entity to map.</param>
    /// <returns>A <see cref="CommentDto"/> representing the provided entity.</returns>
    public static partial CommentDto Map(CommentEntity entity);

    /// <summary>
    /// Maps a <see cref="UserEntity"/> to a <see cref="UserBriefDto"/>.
    /// </summary>
    /// <param name="entity">The user entity to map.</param>
    /// <returns>A <see cref="UserBriefDto"/> representing the provided entity.</returns>
    public static partial UserBriefDto Map(UserEntity entity);

    /// <summary>
    /// Maps a collection of <see cref="CommentEntity"/> objects to a list of <see cref="CommentDto"/> objects.
    /// </summary>
    /// <param name="entities">The collection of comment entities to map.</param>
    /// <returns>A list of <see cref="CommentDto"/> representing the provided entities.</returns>
    public static partial IReadOnlyCollection<CommentDto> Map(IReadOnlyCollection<CommentEntity> entities);
}
