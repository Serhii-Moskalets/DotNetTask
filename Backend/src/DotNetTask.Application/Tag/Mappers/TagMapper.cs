using DotNetTask.Application.Common.Dtos;
using DotNetTask.Domain.Entities;

using Riok.Mapperly.Abstractions;

namespace DotNetTask.Application.Tag.Mappers;

/// <summary>
/// Provides mapping methods to convert <see cref="TagEntity"/> instances
/// to <see cref="TagDto"/> instances.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class TagMapper
{
    /// <summary>
    /// Maps a single <see cref="TagEntity"/> to a <see cref="TagDto"/>.
    /// </summary>
    /// <param name="entity">The tag entity to map.</param>
    /// <returns>A <see cref="TagDto"/> representing the provided entity.</returns>
    public static partial TagDto Map(TagEntity entity);

    /// <summary>
    /// Maps a collection of <see cref="TagEntity"/> instances to a list of <see cref="TagDto"/>.
    /// </summary>
    /// <param name="entities">The collection of tag entities to map.</param>
    /// <returns>A list of <see cref="TagDto"/> representing the provided entities.</returns>
    public static partial IReadOnlyCollection<TagDto> Map(IReadOnlyCollection<TagEntity> entities);
}
