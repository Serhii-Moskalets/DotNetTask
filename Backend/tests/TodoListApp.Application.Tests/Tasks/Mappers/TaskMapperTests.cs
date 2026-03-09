using FluentAssertions;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Tasks.Mappers;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Tasks.Mappers;

/// <summary>
/// Unit tests for <see cref="TaskMapper"/>.
/// Verifies that domain entities are correctly mapped to their respective DTOs.
/// </summary>
public class TaskMapperTests
{
    private static readonly TaskTitle Title = TaskTitle.Create("Task title");
    private static readonly TaskDescription Description = TaskDescription.Create("Task description");
    private readonly string _passwordHash = new('a', 64);

    /// <summary>
    /// Verifies that <see cref="TaskEntity"/> is correctly mapped to <see cref="TaskDto"/>
    /// including its nested objects like Tag.
    /// </summary>
    [Fact]
    public void Map_ToTaskDto_ShouldMapAllFields()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var entity = new TaskEntity(userId, taskListId, Title, DateTime.UtcNow.AddDays(1), Description);

        // Act
        var dto = TaskMapper.Map(entity);

        // Assert
        dto.Should().NotBeNull();
        dto.Title.Should().Be(entity.Title.Value);
        dto.Description.Should().Be(entity.Description!.Value);
        dto.Status.Should().Be(entity.Status);
        dto.DueDate.Should().Be(entity.DueDate);
        dto.CreatedDate.Should().Be(entity.CreatedDate);
    }

    /// <summary>
    /// Verifies that <see cref="TaskEntity"/> is correctly mapped to <see cref="TaskBriefDto"/>,
    /// ensuring that optional nested objects like Tag are handled.
    /// </summary>
    [Fact]
    public void MapToBrief_ShouldMapEssentialFieldsAndTag()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entity = new TaskEntity(userId, Guid.NewGuid(), Title);

        // Act
        var dto = TaskMapper.MapToBrief(entity);

        // Assert
        dto.Should().NotBeNull();
        dto.Title.Should().Be(entity.Title.Value);
        dto.Status.Should().Be(entity.Status);
        dto.Tag.Should().BeNull();
    }

    /// <summary>
    /// Verifies mapping of a collection of entities to a collection of brief DTOs.
    /// </summary>
    [Fact]
    public void MapToBrief_Collection_ShouldMapAllItems()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entities = new List<TaskEntity>
        {
            new(userId, Guid.NewGuid(), TaskTitle.Create("Task 1")),
            new(userId, Guid.NewGuid(), TaskTitle.Create("Task 2")),
        };

        // Act
        var dtos = TaskMapper.MapToBrief(entities);

        // Assert
        dtos.Should().HaveCount(entities.Count);
        dtos.Should().ContainSingle(d => d.Title == "Task 1");
        dtos.Should().ContainSingle(d => d.Title == "Task 2");
    }

    /// <summary>
    /// Verifies that <see cref="CommentEntity"/> is correctly mapped to <see cref="CommentDto"/>.
    /// </summary>
    [Fact]
    public void Map_CommentEntityToDto_ShouldMapFields()
    {
        // Arrange
        var taskId = Guid.NewGuid();

        var user = new UserEntity("Joe", "joes", "joe@gmail.com", this._passwordHash);

        var entity = new CommentEntity(taskId, user.Id, CommentContent.Create("Content"), user);

        // Act
        var dto = TaskMapper.Map(entity);

        // Assert
        dto.Should().NotBeNull();
        dto.Content.Should().Be(entity.Content.Value);
        dto.CreatedDate.Should().Be(entity.CreatedDate);
    }
}
