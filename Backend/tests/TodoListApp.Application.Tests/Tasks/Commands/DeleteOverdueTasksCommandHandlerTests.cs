using FluentAssertions;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tasks.Commands.DeleteOverdueTasks;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteOverdueTasksCommandHandler"/>.
/// Verifies the behavior of the handler for deleting task.
/// </summary>
public class DeleteOverdueTasksCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITaskRepository> _taskRepoMock;
    private readonly Mock<ITaskListRepository> _taskListRepoMock;
    private readonly DeleteOverdueTasksCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteOverdueTasksCommandHandlerTests"/> class.
    /// </summary>
    public DeleteOverdueTasksCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._taskRepoMock = new Mock<ITaskRepository>();
        this._taskListRepoMock = new Mock<ITaskListRepository>();

        this._uowMock.Setup(u => u.Tasks).Returns(this._taskRepoMock.Object);
        this._uowMock.Setup(u => u.TaskLists).Returns(this._taskListRepoMock.Object);

        this._handler = new DeleteOverdueTasksCommandHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Ensures the handler returns a not-found error when the task list does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTaskListDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();

        this._taskListRepoMock
            .Setup(r => r.GetTaskListByIdForUserAsync(taskListId, userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskListEntity?)null);

        this._uowMock.Setup(u => u.TaskLists).Returns(this._taskListRepoMock.Object);

        var command = new DeleteOverdueTasksCommand(taskListId, userId);

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);

        this._taskRepoMock.Verify(
            r => r.DeleteOverdueTaskAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Ensures the handler deletes overdue tasks when the task list exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteOverdueTasks_WhenTaskListExistsAndTasksAreDeleted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedDeletedCount = 5;
        var taskList = new TaskListEntity(userId, "My list");

        this._taskListRepoMock
            .Setup(r => r.GetTaskListByIdForUserAsync(taskList.Id, userId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskList);

        this._taskRepoMock
            .Setup(r => r.DeleteOverdueTaskAsync(taskList.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDeletedCount);

        var command = new DeleteOverdueTasksCommand(taskList.Id, userId);

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDeletedCount);

        this._taskRepoMock.Verify(
            r => r.DeleteOverdueTaskAsync(
            taskList.Id,
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Once);

        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
