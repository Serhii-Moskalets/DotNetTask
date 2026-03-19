using FluentAssertions;
using Moq;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tasks.Commands.DeleteTask;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTaskCommandHandler"/>.
/// Verifies the behavior of the handler for deleting a task.
/// </summary>
public class DeleteTaskCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITaskRepository> _taskRepoMock;
    private readonly DeleteTaskCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskCommandHandlerTests"/> class.
    /// </summary>
    public DeleteTaskCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._taskRepoMock = new Mock<ITaskRepository>();

        this._uowMock.Setup(u => u.Tasks).Returns(this._taskRepoMock.Object);
        this._handler = new DeleteTaskCommandHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Ensures the handler returns a not-found error when the task does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        this._taskRepoMock.Setup(r => r.GetTaskByIdForUserAsync(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
                    .ReturnsAsync((TaskEntity?)null);

        this._uowMock.Setup(u => u.Tasks).Returns(this._taskRepoMock.Object);

        var command = new DeleteTaskCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Be(TaskPolicy.NotFoundMessage);
    }

    /// <summary>
    /// Ensures the handler deletes the task when it exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteTask_WhenTaskExists()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var task = new TaskEntity(ownerId, taskListId, TaskTitle.Create("Title"));

        this._taskRepoMock.Setup(r => r.GetTaskByIdForUserAsync(taskId, ownerId, false, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(task);

        this._uowMock.Setup(u => u.Tasks).Returns(this._taskRepoMock.Object);
        this._uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteTaskCommand(taskId, ownerId);

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        this._taskRepoMock.Verify(r => r.DeleteAsync(task, It.IsAny<CancellationToken>()), Times.Once);
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
