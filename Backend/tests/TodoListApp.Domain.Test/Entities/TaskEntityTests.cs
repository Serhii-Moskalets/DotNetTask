using FluentAssertions;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Enums;
using TodoListApp.Domain.Exceptions;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Test.Entities;

/// <summary>
/// Unit tests for the <see cref="TaskEntity"/> domain entity.
/// </summary>
public class TaskEntityTests
{
    private static readonly TaskTitle Title = TaskTitle.Create("Task title");
    private static readonly TaskTitle NewTitle = TaskTitle.Create("New title");
    private static readonly TaskDescription Description = TaskDescription.Create("Description description");

    /// <summary>
    /// Verifies that the constructor creates a task
    /// when all valid parameters are provided.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateTask_WhenValidData()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddMinutes(5);

        // Act
        var task = new TaskEntity(ownerId, taskListId, Title, dueDate, Description);

        // Assert
        task.OwnerId.Should().Be(ownerId);
        task.TaskListId.Should().Be(taskListId);
        task.Title.Should().Be(Title);
        task.DueDate.Should().Be(dueDate);
        task.Description.Should().Be(Description);
        task.Status.Should().Be(StatusTask.NotStarted);
        task.CreatedDate.Should().BeBefore(DateTime.UtcNow.AddMilliseconds(1));
    }

    /// <summary>
    /// Verifies that a task can be created without a due date.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateTask_WithoutDueDate_WhenValidData()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();

        // Act
        var task = new TaskEntity(ownerId, taskListId, Title, description: Description);

        // Assert
        task.OwnerId.Should().Be(ownerId);
        task.TaskListId.Should().Be(taskListId);
        task.Title.Should().Be(Title);
        task.DueDate.Should().BeNull();
        task.Description.Should().Be(Description);
        (DateTime.UtcNow - task.CreatedDate).TotalSeconds.Should().BeLessThan(1);
    }

    /// <summary>
    /// Verifies that a task can be created without a description.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateTask_WithoutDescription_WhenValidData()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddMinutes(5);

        // Act
        var task = new TaskEntity(ownerId, taskListId, Title, dueDate: dueDate);

        // Assert
        task.OwnerId.Should().Be(ownerId);
        task.TaskListId.Should().Be(taskListId);
        task.Title.Should().Be(Title);
        task.DueDate.Should().Be(dueDate);
        task.Description.Should().BeNull();
        (DateTime.UtcNow - task.CreatedDate).TotalSeconds.Should().BeLessThan(1);
    }

    /// <summary>
    /// Verifies that a task can be created without a due date and description.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateTask_WithoutDueDateAndDescription_WhenValidData()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();

        // Act
        var task = new TaskEntity(ownerId, taskListId, Title);

        // Assert
        task.OwnerId.Should().Be(ownerId);
        task.TaskListId.Should().Be(taskListId);
        task.Title.Should().Be(Title);
        task.DueDate.Should().BeNull();
        task.Description.Should().BeNull();
        (DateTime.UtcNow - task.CreatedDate).TotalSeconds.Should().BeLessThan(1);
    }

    /// <summary>
    /// Verifies that the comments and user accesses collections is initialized on creation.
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeCollections()
    {
        // Arrange & Act
        var task = new TaskEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Title);

        // Assert
        task.Comments.Should().NotBeNull().And.BeEmpty();
        task.UserAccesses.Should().NotBeNull().And.BeEmpty();
    }

    /// <summary>
    /// Verifies that task details are updated when only a new title is provided.
    /// </summary>
    [Fact]
    public void Update_ShouldChangeDetails_WhenValid_WithoutDueDateAndDescription()
    {
        // Arrange
        var task = new TaskEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Title);

        // Act
        task.UpdateDetails(NewTitle);

        // Assert
        task.Title.Value.Should().Be("New title");
    }

    /// <summary>
    /// Verifies that the original due date is maintained if the new due date
    /// provided during the update is null.
    /// </summary>
    [Fact]
    public void Update_ShouldKeepOldDueDate_WhenNewDueDateIsNull()
    {
        // Arrange
        var oldDate = DateTime.UtcNow.AddDays(1);
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), Title, oldDate);

        // Act
        task.UpdateDetails(title: null, Description, dueDate: null);

        // Assert
        task.DueDate.Should().Be(oldDate);
    }

    /// <summary>
    /// Verifies that the task description is successfully cleared (set to null)
    /// when null is explicitly passed as the new description value.
    /// </summary>
    [Fact]
    public void Update_ShouldClearDescription_WhenDescriptionIsNull()
    {
        // Arrange
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), Title, description: Description);

        // Act
        task.UpdateDetails(Title, description: null);

        // Assert
        task.Description.Should().BeNull();
    }

    /// <summary>
    /// Verifies that task details are updated when valid title, description,
    /// and due date are provided.
    /// </summary>
    [Fact]
    public void Update_ShouldChangeDetails_WhenValidWithDueDateAndDescription()
    {
        // Arrange
        var task = new TaskEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Title);
        var newDueDate = DateTime.UtcNow.AddMinutes(5);

        // Act
        task.UpdateDetails(NewTitle, Description, newDueDate);

        // Assert
        task.Title.Value.Should().Be("New title");
        task.Description.Should().Be(Description);
        task.DueDate.Should().Be(newDueDate);
    }

    /// <summary>
    /// Verifies that a task can transition from InProgress to Done status.
    /// </summary>
    [Fact]
    public void ChangeStatus_ShouldCompleteTask_WhenInProgress()
    {
        // Arrange
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), Title);

        // Act
        task.ChangeStatus(StatusTask.InProgress);
        task.ChangeStatus(StatusTask.Done);

        // Assert
        task.Status.Should().Be(StatusTask.Done);
    }

    /// <summary>
    /// Verifies that changing task status from Done back to InProgress
    /// throws a <see cref="DomainException"/>.
    /// </summary>
    [Fact]
    public void ChangeStatus_ShouldThrow_WhenDoneToInProgress()
    {
        // Arrange
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), Title);

        // Act
        task.ChangeStatus(StatusTask.InProgress);
        task.ChangeStatus(StatusTask.Done);

        // Assert
        task.Invoking(t => t.ChangeStatus(StatusTask.InProgress))
            .Should().Throw<DomainException>();
    }

    /// <summary>
    /// Verifies that changing task status to the same value
    /// does not modify the task state.
    /// </summary>
    [Fact]
    public void ChangeStatus_ShouldDoNothing_WhenSameStatus()
    {
        // Arrange
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), Title);

        // Assert & Act
        task.ChangeStatus(StatusTask.NotStarted);
        task.Status.Should().Be(StatusTask.NotStarted);

        task.ChangeStatus(StatusTask.NotStarted);
        task.Status.Should().Be(StatusTask.NotStarted);

        task.ChangeStatus(StatusTask.InProgress);
        task.ChangeStatus(StatusTask.InProgress);
        task.Status.Should().Be(StatusTask.InProgress);
    }

    /// <summary>
    /// Verifies that providing an invalid status enum
    /// throws a <see cref="DomainException"/>.
    /// </summary>
    [Fact]
    public void ChangeStatus_ShouldThrow_WhenInvalidEnum()
    {
        // Arrange
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), Title);
        const StatusTask invalidStatus = (StatusTask)999;

        // Assert & Act
        task.Invoking(t => t.ChangeStatus(invalidStatus))
            .Should().Throw<DomainException>();
    }

    /// <summary>
    /// Verifies that setting a tag assigns the tag ID
    /// and resets the navigation property.
    /// </summary>
    [Fact]
    public void SetTag_ShouldUpdateTagIdAndResetTag()
    {
        // Arrange
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), Title);
        var tagId = Guid.NewGuid();

        // Act
        task.SetTag(tagId);

        // Assert
        task.TagId.Should().Be(tagId);
        task.Tag.Should().BeNull();
    }

    /// <summary>
    /// Verifies that setting a null tag removes the tag
    /// from the task.
    /// </summary>
    [Fact]
    public void SetTag_ShouldRemoveTag_WhenNull()
    {
        // Arrange
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), Title);
        var tagId = Guid.NewGuid();
        task.SetTag(tagId);

        // Act
        task.SetTag(null);

        // Assert
        task.TagId.Should().BeNull();
        task.Tag.Should().BeNull();
    }
}
