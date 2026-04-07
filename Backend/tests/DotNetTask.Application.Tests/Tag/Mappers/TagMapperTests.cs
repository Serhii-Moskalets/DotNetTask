using DotNetTask.Application.Common.Dtos;
using DotNetTask.Application.Tag.Mappers;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using TinyResult;

namespace DotNetTask.Application.Tests.Tag.Mappers;

/// <summary>
/// Unit tests for <see cref="TagMapper"/>.
/// Verifies that the entity is correctly mapped to the DTO.
/// </summary>
public class TagMapperTests
{
    /// <summary>
    /// Tests that a single <see cref="TagEntity"/> maps correctly to <see cref="TagDto"/>.
    /// </summary>
    [Fact]
    public void Map_EntityToDto_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TagEntity entity = new(TagName.Create("Work"), userId);
        Guid tagId = Guid.NewGuid();
        typeof(TagEntity).GetProperty(nameof(TagEntity.Id))?.SetValue(entity, tagId);

        // Act
        TagDto result = TagMapper.Map(entity);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(tagId);
        result.Name.Should().Be("Work");
    }

    /// <summary>
    /// Tests that a collection of <see cref="TagEntity"/> maps correctly to a collection of <see cref="TagDto"/>.
    /// </summary>
    [Fact]
    public void Map_CollectionToDtoList_ShouldMapAllItems()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        List<TagEntity> entities = new()
        {
            new(TagName.Create("Urgent"), userId),
            new(TagName.Create("Personal"), userId),
            new(TagName.Create("Study"), userId),
        };

        // Act
        IReadOnlyCollection<TagDto> result = TagMapper.Map(entities);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Select(x => x.Name)
            .Should()
            .ContainInOrder("Urgent", "Personal", "Study");
    }

    /// <summary>
    /// Tests that an empty collection of entities maps to an empty collection of DTOs.
    /// </summary>
    [Fact]
    public void Map_EmptyCollection_ShouldReturnEmptyList()
    {
        // Arrange
        List<TagEntity> entities = new();

        // Act
        IReadOnlyCollection<TagDto> result = TagMapper.Map(entities);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
