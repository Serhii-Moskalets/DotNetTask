using FluentAssertions;
using TodoListApp.Application.UserTaskAccess.Mappers;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.UserTaskAccess.Mappers;

/// <summary>
/// Unit tests for the <see cref="TaskAccessForUserMapper"/> class.
/// </summary>
public class TaskAccessForUserMapperTests
{
    /// <summary>
    /// Verifies that the mapper correctly extracts and maps the nested <see cref="TaskEntity"/>
    /// from a <see cref="UserTaskAccessEntity"/>.
    /// </summary>
    [Fact]
    public void Map_UserTaskAccessEntity_ShouldMapInnerTaskCorrectly()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var task = new TaskEntity(
            ownerId,
            taskListId: Guid.NewGuid(),
            TaskTitle.Create("Shared Task"),
            dueDate: DateTime.UtcNow.AddDays(1),
            TaskDescription.Create("Task Description"));

        var accessEntity = new UserTaskAccessEntity(task.Id, userId)
        {
            Task = task,
        };

        // Act
        var result = TaskAccessForUserMapper.Map(accessEntity);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(task.Id);
        result.Title.Should().Be(task.Title.Value);
        result.Description.Should().Be(task.Description!.Value);
        result.DueDate.Should().Be(task.DueDate);
    }

    /// <summary>
    /// Verifies that a collection of access entities is correctly mapped to a collection of Task DTOs.
    /// </summary>
    [Fact]
    public void Map_Collection_ShouldReturnMappedTaskDtos()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var task1 = new TaskEntity(ownerId, Guid.NewGuid(), TaskTitle.Create("Task 1"));
        var task2 = new TaskEntity(ownerId, Guid.NewGuid(), TaskTitle.Create("Task 2"));

        var entities = new List<UserTaskAccessEntity>
        {
            new(task1.Id, userId) { Task = task1 },
            new(task2.Id, userId) { Task = task2 },
        };

        // Act
        var result = TaskAccessForUserMapper.Map(entities);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Select(r => r.Title).Should().Contain(["Task 1", "Task 2"]);
    }

    /// <summary>
    /// Verifies that an empty collection returns an empty list.
    /// </summary>
    [Fact]
    public void Map_EmptyCollection_ShouldReturnEmptyList()
    {
        // Arrange
        var entities = new List<UserTaskAccessEntity>();

        // Act
        var result = TaskAccessForUserMapper.Map(entities);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that a <see cref="TaskEntity"/> can be mapped directly to a DTO.
    /// </summary>
    [Fact]
    public void Map_TaskEntityDirectly_ShouldMapToTaskDto()
    {
        // Arrange
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), TaskTitle.Create("Direct Map"));

        // Act
        var result = TaskAccessForUserMapper.Map(task);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Direct Map");
    }
}
