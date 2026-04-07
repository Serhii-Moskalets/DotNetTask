using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Tasks.Commands.ChangeTaskStatus;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Enums;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tests.Tasks.Commands.ChangeTaskStatus;

/// <summary>
/// Unit tests for <see cref="ChangeTaskStatusCommandHandler"/>.
/// Verifies behavior when changing task status.
/// </summary>
public class ChangeTaskStatusCommandHandlerTests
{
    private static readonly TaskTitle Title = TaskTitle.Create("Title");

    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITaskRepository> _taskRepoMock;
    private readonly ChangeTaskStatusCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeTaskStatusCommandHandlerTests"/> class.
    /// </summary>
    public ChangeTaskStatusCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._taskRepoMock = new Mock<ITaskRepository>();

        this._uowMock.Setup(u => u.Tasks).Returns(this._taskRepoMock.Object);
        this._handler = new ChangeTaskStatusCommandHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Ensures that the handler returns failure if the task does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTaskNotFound()
    {
        // Arrange
        ChangeTaskStatusCommand command = new(Guid.NewGuid(), Guid.NewGuid(), StatusTask.InProgress);

        this._taskRepoMock.Setup(r => r.GetTaskByIdForUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskEntity?)null);

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Be(TaskPolicy.NotFoundMessage);

        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the handler returns a success result without invoking a database save operation
    /// when the requested status is identical to the current status of the task.
    /// </summary>
    /// <remarks>
    /// This test ensures the handler is idempotent and optimizes performance by avoiding
    /// unnecessary transactional overhead (I/O) when no state change is actually required.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenStatusIsAlreadyTheSame()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TaskEntity task = new(userId, Guid.NewGuid(), Title);

        ChangeTaskStatusCommand command = new(task.Id, userId, task.Status);

        this._taskRepoMock
            .Setup(r => r.GetTaskByIdForUserAsync(task.Id, userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Ensures that the handler successfully changes the status of a task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldChangeStatusSuccessfully_WhenTaskExists()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        StatusTask newStatus = StatusTask.InProgress;

        TaskEntity taskEntity = new(userId, Guid.NewGuid(), Title);

        this._taskRepoMock.Setup(r => r.GetTaskByIdForUserAsync(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskEntity);

        this._uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        ChangeTaskStatusCommand command = new(Guid.NewGuid(), userId, newStatus);

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        taskEntity.Status.Should().Be(newStatus);

        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
