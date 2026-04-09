using DotNetTask.Domain.Common;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Enums;
using DotNetTask.Domain.Exceptions;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using TinyResult;

namespace DotNetTask.Domain.Test.Entities;

/// <summary>
/// Unit tests for the <see cref="TaskEntity"/> domain entity.
/// </summary>
public class TaskEntityTests
{
    private static readonly TaskTitle Title = TaskTitle.Create("Task title");
    private static readonly TaskTitle NewTitle = TaskTitle.Create("New title");
    private static readonly TaskDescription Description = TaskDescription.Create("Description description");

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid TaskListId = Guid.NewGuid();

    /// <summary>
    /// Verifies that the constructor creates a task
    /// when all valid parameters are provided.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateTask_WhenValidData()
    {
        // Arrange
        DateTime dueDate = DateTime.UtcNow.AddMinutes(5);

        // Act
        TaskEntity task = CreateTask(dueDate, Description);

        // Assert
        task.OwnerId.Should().Be(UserId);
        task.TaskListId.Should().Be(TaskListId);
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
        // Act
        TaskEntity task = CreateTask(description: Description);

        // Assert
        task.OwnerId.Should().Be(UserId);
        task.TaskListId.Should().Be(TaskListId);
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
        DateTime dueDate = DateTime.UtcNow.AddMinutes(5);

        // Act
        TaskEntity task = CreateTask(dueDate: dueDate);

        // Assert
        task.OwnerId.Should().Be(UserId);
        task.TaskListId.Should().Be(TaskListId);
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
        // Act
        TaskEntity task = CreateTask();

        // Assert
        task.OwnerId.Should().Be(UserId);
        task.TaskListId.Should().Be(TaskListId);
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
        TaskEntity task = CreateTask();

        // Assert
        task.Comments.Should().NotBeNull().And.BeEmpty();
        task.UserAccesses.Should().NotBeNull().And.BeEmpty();
    }

    /// <summary>
    /// Verifies that creating a task with a due date in the past
    /// throws a <see cref="DomainException"/>.
    /// </summary>
    [Fact]
    public void Constructor_ShouldThrow_WhenDueDateInPast()
    {
        // Arrange
        DateTime pastDate = DateTime.UtcNow.AddMinutes(-5);

        // Act
        Action act = () => CreateTask(pastDate);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage(TaskPolicy.InvalidDueDateMessage);
    }

    /// <summary>
    /// Verifies that task details are updated when only a new title is provided.
    /// </summary>
    [Fact]
    public void Update_ShouldChangeDetails_WhenValid_WithoutDueDateAndDescription()
    {
        // Arrange
        TaskEntity task = CreateTask();

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
        DateTime oldDate = DateTime.UtcNow.AddDays(1);
        TaskEntity task = CreateTask(dueDate: oldDate);

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
        TaskEntity task = CreateTask(description: Description);

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
        TaskEntity task = CreateTask();
        DateTime newDueDate = DateTime.UtcNow.AddMinutes(5);

        // Act
        task.UpdateDetails(NewTitle, Description, newDueDate);

        // Assert
        task.Title.Value.Should().Be("New title");
        task.Description.Should().Be(Description);
        task.DueDate.Should().Be(newDueDate);
    }

    /// <summary>
    /// Verifies that updating task details with a due date in the past
    /// throws a <see cref="DomainException"/>.
    /// </summary>
    [Fact]
    public void Update_ShouldThrow_WhenDueDateInPast()
    {
        // Arrange
        TaskEntity task = CreateTask();
        DateTime pastDate = DateTime.UtcNow.AddMinutes(-1);

        // Act
        Action act = () => task.UpdateDetails(Title, Description, pastDate);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage(TaskPolicy.InvalidDueDateMessage);
    }

    /// <summary>
    /// Verifies that a task can transition from InProgress to Done status.
    /// </summary>
    [Fact]
    public void ChangeStatus_ShouldReturnSuccess_WhenTransitionIsValid()
    {
        // Arrange
        TaskEntity task = CreateTask();

        // Act
        Result<Unit> result = task.ChangeStatus(StatusTask.InProgress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Status.Should().Be(StatusTask.InProgress);
    }

    /// <summary>
    /// Verifies that changing task status to the same value
    /// does not modify the task state.
    /// </summary>
    [Fact]
    public void ChangeStatus_ShouldReturnSuccess_WhenSameStatus()
    {
        // Arrange
        TaskEntity task = CreateTask();

        // Act
        Result<Unit> result = task.ChangeStatus(StatusTask.NotStarted);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Status.Should().Be(StatusTask.NotStarted);
    }

    /// <summary>
    /// Verifies that providing an invalid status enum
    /// throws a <see cref="DomainException"/>.
    /// </summary>
    [Fact]
    public void ChangeStatus_ShouldReturnFailure_WhenInvalidEnum()
    {
        // Arrange
        TaskEntity task = CreateTask();
        const StatusTask invalidStatus = (StatusTask)999;

        // Act
        Result<Unit> result = task.ChangeStatus(invalidStatus);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be(TaskPolicy.InvalidStatusMessage);
    }

    /// <summary>
    /// Verifies that changing task status from <see cref="StatusTask.Done"/>
    /// to <see cref="StatusTask.NotStarted"/> returns a failure result.
    /// </summary>
    [Fact]
    public void ChangeStatus_ShouldReturnFailure_WhenDoneToNotStarted()
    {
        // Arrange
        TaskEntity task = CreateTask();
        task.ChangeStatus(StatusTask.InProgress);
        task.ChangeStatus(StatusTask.Done);

        // Act
        Result<Unit> result = task.ChangeStatus(StatusTask.NotStarted);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be(TaskPolicy.DoneToNotStartedMessage);
    }

    /// <summary>
    /// Verifies that changing task status from <see cref="StatusTask.InProgress"/>
    /// to <see cref="StatusTask.NotStarted"/> returns a failure result.
    /// </summary>
    [Fact]
    public void ChangeStatus_ShouldReturnFailure_WhenInProgressToNotStarted()
    {
        // Arrange
        TaskEntity task = CreateTask();
        task.ChangeStatus(StatusTask.InProgress);

        // Act
        Result<Unit> result = task.ChangeStatus(StatusTask.NotStarted);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be(TaskPolicy.InProgressToNotStartedMessage);
    }

    /// <summary>
    /// Verifies that a task can successfully complete the full lifecycle:
    /// NotStarted → InProgress → Done.
    /// </summary>
    [Fact]
    public void ChangeStatus_ShouldFollowFullLifecycle()
    {
        // Arrange
        TaskEntity task = CreateTask();

        // Assert
        task.ChangeStatus(StatusTask.InProgress);
        task.ChangeStatus(StatusTask.Done);

        task.Status.Should().Be(StatusTask.Done);
    }

    /// <summary>
    /// Verifies that changing task status directly to <see cref="StatusTask.Done"/>
    /// without first setting it to <see cref="StatusTask.InProgress"/> returns a failure result.
    /// </summary>
    [Fact]
    public void ChangeStatus_ShouldReturnFailure_WhenCompletingWithoutInProgress()
    {
        // Arrange
        TaskEntity task = CreateTask();

        // Act
        Result<Unit> result = task.ChangeStatus(StatusTask.Done);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be(TaskPolicy.CompletionRequiresInProgressMessage);
    }

    /// <summary>
    /// Verifies that setting a tag assigns the tag ID
    /// and resets the navigation property.
    /// </summary>
    [Fact]
    public void SetTag_ShouldUpdateTagIdAndResetTag()
    {
        // Arrange
        TaskEntity task = CreateTask();
        Guid tagId = Guid.NewGuid();

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
        TaskEntity task = CreateTask();
        Guid tagId = Guid.NewGuid();
        task.SetTag(tagId);

        // Act
        task.SetTag(null);

        // Assert
        task.TagId.Should().BeNull();
        task.Tag.Should().BeNull();
    }

    /// <summary>
    /// Verifies that setting the same tag ID does not modify the task state.
    /// </summary>
    [Fact]
    public void SetTag_ShouldDoNothing_WhenSameTagId()
    {
        // Arrange
        TaskEntity task = CreateTask();
        Guid tagId = Guid.NewGuid();
        task.SetTag(tagId);

        // Act
        task.SetTag(tagId);

        // Assert
        task.TagId.Should().Be(tagId);
    }

    private static TaskEntity CreateTask(DateTime? dueDate = null, TaskDescription? description = null) => new(UserId, TaskListId, Title, dueDate, description);
}
