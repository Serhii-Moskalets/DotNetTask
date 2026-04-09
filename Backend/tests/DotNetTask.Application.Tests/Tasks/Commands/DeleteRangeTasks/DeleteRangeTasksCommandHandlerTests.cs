using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Tasks.Commands.DeleteRangeTasks;
using DotNetTask.Domain.Constants;
using FluentAssertions;
using Moq;
using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tests.Tasks.Commands.DeleteRangeTasks;

/// <summary>
/// Unit tests for <see cref="DeleteRangeTasksCommandHandler"/>.
/// Verifies the behavior of the handler for deleting task.
/// </summary>
public class DeleteRangeTasksCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITaskRepository> _taskRepoMock;
    private readonly DeleteRangeTasksCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteRangeTasksCommandHandlerTests"/> class.
    /// </summary>
    public DeleteRangeTasksCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._taskRepoMock = new Mock<ITaskRepository>();

        this._uowMock.Setup(u => u.Tasks).Returns(this._taskRepoMock.Object);

        this._handler = new DeleteRangeTasksCommandHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Should return validation error when not all tasks belong to the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserDoesNotOwnAllTasks()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        List<Guid> taskIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];

        this._taskRepoMock
            .Setup(r => r.CountOwnedTasksAsync(taskIds, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2); // user owns only 2 of 3

        DeleteRangeTasksCommand command = new(taskIds, userId);

        // Act
        Result<int> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.ValidationError);
        result.Error.Message.Should().Be(TaskPolicy.AccessDeniedMessage);

        this._taskRepoMock.Verify(
            r => r.DeleteRangeAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Should delete tasks when user owns all provided task IDs.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteTasks_WhenUserOwnsAllTasks()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        List<Guid> taskIds = [Guid.NewGuid(), Guid.NewGuid()];
        int expectedDeletedCount = taskIds.Count;

        this._taskRepoMock
            .Setup(r => r.CountOwnedTasksAsync(taskIds, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskIds.Count);

        this._taskRepoMock
            .Setup(r => r.DeleteRangeAsync(taskIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDeletedCount);

        DeleteRangeTasksCommand command = new(taskIds, userId);

        // Act
        Result<int> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDeletedCount);

        this._taskRepoMock.Verify(
            r => r.DeleteRangeAsync(taskIds, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
