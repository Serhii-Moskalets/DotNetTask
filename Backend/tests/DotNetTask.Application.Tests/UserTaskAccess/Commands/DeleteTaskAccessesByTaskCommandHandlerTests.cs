using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.UserTaskAccess.Commands.DeleteTaskAccessesByTask;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;
using FluentAssertions;
using Moq;

using TinyResult;

namespace DotNetTask.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTaskAccessesByTaskCommandHandler"/>.
/// Verifies behavior for deleting all user-task access entries for a specific task.
/// </summary>
public class DeleteTaskAccessesByTaskCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITaskRepository> _tasksRepoMock;
    private readonly Mock<IUserTaskAccessRepository> _userTaskAccessRepoMock;
    private readonly DeleteTaskAccessesByTaskCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskAccessesByTaskCommandHandlerTests"/> class.
    /// </summary>
    public DeleteTaskAccessesByTaskCommandHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._tasksRepoMock = new Mock<ITaskRepository>();
        this._userTaskAccessRepoMock = new Mock<IUserTaskAccessRepository>();

        this._unitOfWorkMock.Setup(u => u.Tasks).Returns(this._tasksRepoMock.Object);
        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses).Returns(this._userTaskAccessRepoMock.Object);

        this._handler = new DeleteTaskAccessesByTaskCommandHandler(this._unitOfWorkMock.Object);
    }

    /// <summary>
    /// Ensures the handler returns a failure result when the task does not exist
    /// or the user is not the owner of the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTaskNotFoundOrUserNotOwner()
    {
        // Arrange
        Guid taskId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        DeleteTaskAccessesByTaskCommand command = new(taskId, userId);

        this._tasksRepoMock.Setup(r => r.GetTaskByIdForUserAsync(taskId, userId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync((TaskEntity?)null);

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be(UserTaskAccessPolicy.AccessDeniedMessage);
        this._userTaskAccessRepoMock.Verify(r => r.DeleteAllByTaskIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the handler successfully removes all shared access entries associated with a task
    /// when the task exists and the requesting user is the authorized owner.
    /// </summary>
    /// <remarks>
    /// This test ensures that the "Delete All" logic correctly orchestrates the retrieval
    /// of the task for the specific user and invokes the repository's bulk deletion method
    /// followed by a unit of work commit.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteAllAccesses_WhenTaskExistsAndHasSharedAccesses()
    {
        // Arrange
        Guid taskId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        TaskEntity task = new(userId, Guid.NewGuid(), TaskTitle.Create("Test Task"));
        DeleteTaskAccessesByTaskCommand command = new(taskId, userId);

        this._tasksRepoMock.Setup(r => r.GetTaskByIdForUserAsync(taskId, userId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(task);

        this._userTaskAccessRepoMock.Setup(r => r.ExistsByTaskIdAsync(taskId, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(true);

        this._userTaskAccessRepoMock.Setup(r => r.DeleteAllByTaskIdAsync(taskId, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(5);

        this._unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this._userTaskAccessRepoMock.Verify(r => r.DeleteAllByTaskIdAsync(taskId, It.IsAny<CancellationToken>()), Times.Once);
        this._unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
