using DotNetTask.Application.Common.Dtos;
using DotNetTask.Application.UserTaskAccess.Mappers;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

namespace DotNetTask.Application.Tests.UserTaskAccess.Mappers;

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
        Guid ownerId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        TaskEntity task = new(
            ownerId,
            taskListId: Guid.NewGuid(),
            TaskTitle.Create("Shared Task"),
            dueDate: DateTime.UtcNow.AddDays(1),
            TaskDescription.Create("Task Description"));

        UserTaskAccessEntity accessEntity = new(task.Id, userId)
        {
            Task = task,
        };

        // Act
        TaskDto result = TaskAccessForUserMapper.Map(accessEntity);

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
        Guid ownerId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        TaskEntity task1 = new(ownerId, Guid.NewGuid(), TaskTitle.Create("Task 1"));
        TaskEntity task2 = new(ownerId, Guid.NewGuid(), TaskTitle.Create("Task 2"));

        List<UserTaskAccessEntity> entities = new()
        {
            new(task1.Id, userId) { Task = task1 },
            new(task2.Id, userId) { Task = task2 },
        };

        // Act
        IReadOnlyCollection<TaskDto> result = TaskAccessForUserMapper.Map(entities);

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
        List<UserTaskAccessEntity> entities = new();

        // Act
        IReadOnlyCollection<TaskDto> result = TaskAccessForUserMapper.Map(entities);

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
        TaskEntity task = new(Guid.NewGuid(), Guid.NewGuid(), TaskTitle.Create("Direct Map"));

        // Act
        TaskDto result = TaskAccessForUserMapper.Map(task);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Direct Map");
    }
}
