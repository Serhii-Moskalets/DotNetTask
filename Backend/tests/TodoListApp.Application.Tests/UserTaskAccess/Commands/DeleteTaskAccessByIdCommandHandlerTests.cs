using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.Services;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessById;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTaskAccessByIdCommandHandler"/>.
/// Ensures that task access revocation is only performed after verifying the existence of the relationship.
/// </summary>
public class DeleteTaskAccessByIdCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITaskRepository> _taskRepoMock;
    private readonly Mock<IUserTaskAccessRepository> _accessRepoMock;
    private readonly DeleteTaskAccessByIdCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskAccessByIdCommandHandlerTests"/> class.
    /// </summary>
    public DeleteTaskAccessByIdCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._taskRepoMock = new Mock<ITaskRepository>();
        this._accessRepoMock = new Mock<IUserTaskAccessRepository>();

        this._uowMock.Setup(u => u.Tasks).Returns(this._taskRepoMock.Object);
        this._uowMock.Setup(u => u.UserTaskAccesses).Returns(this._accessRepoMock.Object);

        this._handler = new DeleteTaskAccessByIdCommandHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Verifies that the handler returns a failure result with <see cref="ErrorCode.ValidationError"/>
    /// if the service indicates that no access relationship exists between the user and the task.
    /// </summary>
    /// <remarks>
    /// This test ensures that the system prevents unnecessary or invalid deletion attempts
    /// and provides meaningful feedback when access does not exist.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserHasNoAccess()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var command = new DeleteTaskAccessByIdCommand(taskId, userId, ownerId);

        this._taskRepoMock.Setup(r => r.IsTaskOwnerAsync(taskId, ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.InvalidOperation, result.Error!.Code);
        Assert.Equal(UserTaskAccessPolicy.AccessDeniedMessage, result.Error.Message);

        this._accessRepoMock.Verify(r => r.DeleteByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the handler successfully deletes the access record and persists the change
    /// when the <see cref="IUserTaskAccessService"/> confirms the user currently has access to the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteAccess_WhenUserHasAccess()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var command = new DeleteTaskAccessByIdCommand(taskId, userId, ownerId);

        this._taskRepoMock.Setup(r => r.IsTaskOwnerAsync(taskId, ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        this._uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        this._accessRepoMock.Verify(r => r.DeleteByIdAsync(taskId, userId, It.IsAny<CancellationToken>()), Times.Once);
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}