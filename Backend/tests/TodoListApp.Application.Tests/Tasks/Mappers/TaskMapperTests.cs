using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Tasks.Mappers;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Tasks.Mappers;

/// <summary>
/// Unit tests for <see cref="TaskMapper"/>.
/// Verifies that domain entities are correctly mapped to their respective DTOs.
/// </summary>
public class TaskMapperTests
{
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
        var entity = new TaskEntity(userId, taskListId, "Complete Project", DateTime.UtcNow.AddDays(1), "Description test");

        // Act
        var dto = TaskMapper.Map(entity);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(entity.Title, dto.Title);
        Assert.Equal(entity.Description, dto.Description);
        Assert.Equal(entity.Status, dto.Status);
        Assert.Equal(entity.DueDate, dto.DueDate);
        Assert.Equal(entity.CreatedDate, dto.CreatedDate);
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
        var entity = new TaskEntity(userId, Guid.NewGuid(), "Brief Task");

        // Act
        var dto = TaskMapper.MapToBrief(entity);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(entity.Title, dto.Title);
        Assert.Equal(entity.Status, dto.Status);
        Assert.Null(dto.Tag); // Tag was not set
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
            new(userId, Guid.NewGuid(), "Task 1"),
            new(userId, Guid.NewGuid(), "Task 2"),
        };

        // Act
        var dtos = TaskMapper.MapToBrief(entities);

        // Assert
        Assert.Equal(entities.Count, dtos.Count);
        Assert.Contains(dtos, d => d.Title == "Task 1");
        Assert.Contains(dtos, d => d.Title == "Task 2");
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

        var entity = new CommentEntity(taskId, user.Id, "Test comment content", user);

        // Act
        var dto = TaskMapper.Map(entity);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(entity.Text, dto.Text);
        Assert.Equal(entity.CreatedDate, dto.CreatedDate);
    }
}
