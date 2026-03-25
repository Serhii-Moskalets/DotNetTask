using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Tasks.Commands.UpdateTask;
using DotNetTask.Application.Tasks.Dtos;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="UpdateTaskCommandHandler"/>.
/// Ensures that the handler correctly validates, finds, and updates a task.
/// </summary>
public class UpdateTaskCommandHandlerTests
{
    private static readonly TaskTitle Title = TaskTitle.Create("Title");

    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITaskRepository> _taskRepoMock;
    private readonly UpdateTaskCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTaskCommandHandlerTests"/> class.
    /// </summary>
    public UpdateTaskCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._taskRepoMock = new Mock<ITaskRepository>();

        this._uowMock.Setup(u => u.Tasks).Returns(this._taskRepoMock.Object);
        this._handler = new UpdateTaskCommandHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Ensures the handler returns a not-found error when the task does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        this._taskRepoMock.Setup(
            r =>
            r.GetTaskByIdForUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskEntity?)null);

        UpdateTaskDto dto = new UpdateTaskDto
        {
            TaskId = Guid.NewGuid(),
            Title = "New Title",
        };

        UpdateTaskCommand command = new UpdateTaskCommand(dto, Guid.NewGuid());

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Be(TaskPolicy.NotFoundMessage);
    }

    /// <summary>
    /// Ensures the handler updates the task and saves changes when the command is valid.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldUpdateTask_WhenCommandIsValid()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid taskListId = Guid.NewGuid();

        TaskEntity task = new TaskEntity(userId, taskListId, Title);

        this._taskRepoMock.Setup(
            r =>
            r.GetTaskByIdForUserAsync(
                task.Id,
                userId,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        this._uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        UpdateTaskDto dto = new UpdateTaskDto
        {
            TaskId = task.Id,
            Title = "New Title",
            Description = "New Description",
            DueDate = DateTime.UtcNow.AddDays(1),
        };

        UpdateTaskCommand command = new UpdateTaskCommand(dto, userId);

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        task.Title.Value.Should().Be("New Title");
        task.Description?.Value.Should().Be("New Description");
        task.DueDate.Should().Be(dto.DueDate);

        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the handler avoids unnecessary database writes by checking for data equality.
    /// If the incoming DTO data is identical to the current entity state, <c>SaveChangesAsync</c> should not be called.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
    [Fact]
    public async Task Handle_ShouldNotSave_WhenDataIsIdentical()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TaskEntity task = new TaskEntity(userId, Guid.NewGuid(), Title);

        this._taskRepoMock.Setup(
            r =>
            r.GetTaskByIdForUserAsync(
                task.Id,
                userId,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        UpdateTaskDto dto = new UpdateTaskDto
        {
            TaskId = task.Id,
            Title = Title.Value,
            Description = null,
            DueDate = task.DueDate,
        };

        UpdateTaskCommand command = new UpdateTaskCommand(dto, userId);

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the handler does not save changes when Title is null (not provided)
    /// and other fields match the current entity state.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldNotSave_WhenTitleIsNullAndOtherFieldsMatch()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TaskEntity task = new TaskEntity(userId, Guid.NewGuid(), Title);

        this._taskRepoMock.Setup(
            r =>
            r.GetTaskByIdForUserAsync(
                task.Id,
                userId,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        UpdateTaskDto dto = new UpdateTaskDto
        {
            TaskId = task.Id,
            Title = null,
            Description = null,
            DueDate = task.DueDate,
        };

        UpdateTaskCommand command = new UpdateTaskCommand(dto, userId);

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the handler updates the task when Title is null but Description or DueDate changes.
    /// Title should remain unchanged.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldUpdate_WhenTitleIsNullButOtherFieldsChanged()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TaskEntity task = new TaskEntity(userId, Guid.NewGuid(), Title);
        TaskDescription newDescription = TaskDescription.Create("New Description");
        DateTime newDueDate = DateTime.UtcNow.AddDays(2);

        this._taskRepoMock.Setup(
            r =>
            r.GetTaskByIdForUserAsync(
                task.Id,
                userId,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        this._uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        UpdateTaskDto dto = new UpdateTaskDto
        {
            TaskId = task.Id,
            Title = null,
            Description = newDescription.Value,
            DueDate = newDueDate,
        };

        UpdateTaskCommand command = new UpdateTaskCommand(dto, userId);

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Title.Should().Be(Title);
        task.Description.Should().Be(newDescription);
        task.DueDate.Should().Be(newDueDate);
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
