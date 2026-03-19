using FluentAssertions;
using TodoListApp.Application.TaskList.Mappers;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.TaskList.Mappers;

/// <summary>
/// Unit tests for <see cref="TaskListMapper"/>.
/// </summary>
public class TaskListMapperTests
{
    /// <summary>
    /// Verifies that <see cref="TaskListMapper.Map(TaskListEntity)"/> correctly maps
    /// properties from entity to DTO.
    /// </summary>
    [Fact]
    public void Map_EntityToDto_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entity = new TaskListEntity(userId, TaskListTitle.Create("My Awesome List"));

        // Act
        var dto = TaskListMapper.Map(entity);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(entity.Id);
        dto.Title.Should().Be(entity.Title.Value);
        dto.OwnerId.Should().Be(entity.OwnerId);
    }

    /// <summary>
    /// Verifies that <see cref="TaskListMapper.Map(IReadOnlyCollection{TaskListEntity})"/>
    /// correctly maps a collection of entities.
    /// </summary>
    [Fact]
    public void Map_CollectionToDtoList_ShouldMapAllItems()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entities = new List<TaskListEntity>
    {
        new(userId, TaskListTitle.Create("List 1")),
        new(userId, TaskListTitle.Create("List 2")),
        new(userId, TaskListTitle.Create("List 3")),
    };

        // Act
        var result = TaskListMapper.Map(entities);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(entities.Count);

        result.Select(x => x.Title).Should().Equal(entities.Select(x => x.Title.Value));
        result.Select(x => x.Id).Should().Equal(entities.Select(x => x.Id));
    }

    /// <summary>
    /// Verifies that the mapper returns an empty collection when provided an empty source.
    /// </summary>
    [Fact]
    public void Map_EmptyCollection_ShouldReturnEmptyCollection()
    {
        // Arrange
        var entities = Array.Empty<TaskListEntity>();

        // Act
        var result = TaskListMapper.Map(entities);

        // Assert
        result.Should().BeEmpty();
    }
}
