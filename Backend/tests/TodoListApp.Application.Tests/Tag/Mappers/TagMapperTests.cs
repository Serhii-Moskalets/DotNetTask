using FluentAssertions;
using TinyResult;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Tag.Mappers;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Tag.Mappers;

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
        var userId = Guid.NewGuid();
        var entity = new TagEntity(TagName.Create("Work"), userId);
        var tagId = Guid.NewGuid();
        typeof(TagEntity).GetProperty(nameof(TagEntity.Id))?.SetValue(entity, tagId);

        // Act
        var result = TagMapper.Map(entity);

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
        var userId = Guid.NewGuid();
        var entities = new List<TagEntity>
        {
            new(TagName.Create("Urgent"), userId),
            new(TagName.Create("Personal"), userId),
            new(TagName.Create("Study"), userId),
        };

        // Act
        var result = TagMapper.Map(entities);

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
        var entities = new List<TagEntity>();

        // Act
        var result = TagMapper.Map(entities);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
