using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

namespace DotNetTask.Domain.Test.Entities;

/// <summary>
/// Unit tests for the <see cref="TaskListEntity"/> domain entity.
/// </summary>
public class TaskListEntityTest
{
    /// <summary>
    /// Verifies that the constructor creates a task list
    /// when valid owner ID and title are provided.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateTaskList_WhenValidData()
    {
        // Arrange
        Guid ownerId = Guid.NewGuid();
        const string title = "My Task List";

        // Act
        TaskListEntity taskList = CreateTaskListEntity(ownerId, title);

        // Assert
        taskList.OwnerId.Should().Be(ownerId);
        taskList.Title.Value.Should().Be(title);
        taskList.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// Verifies that the tasks collection is initialized
    /// when a task list is created.
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeTasksCollection()
    {
        // Act
        TaskListEntity taskList = CreateTaskListEntity(null);

        // Assert
        taskList.Tasks.Should().NotBeNull();
        taskList.Tasks.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that the task list title is updated
    /// when a valid new title is provided.
    /// </summary>
    [Fact]
    public void Update_ShouldChangeTitle()
    {
        // Arrange
        TaskListEntity taskList = CreateTaskListEntity(null);
        TaskListTitle newTitle = TaskListTitle.Create("New title");

        // Act
        taskList.UpdateTitle(newTitle);

        // Assert
        taskList.Title.Should().Be(newTitle);
    }

    private static TaskListEntity CreateTaskListEntity(Guid? owner_id, string title = "My Task List") => new(owner_id ?? Guid.NewGuid(), TaskListTitle.Create(title));
}
