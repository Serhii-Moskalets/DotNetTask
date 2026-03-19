using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessesByUser;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTaskAccessesByUserCommandHandler"/>.
/// Verifies behavior for deleting all user-task access entries for a specific user.
/// </summary>
public class DeleteTaskAccessesByUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserTaskAccessRepository> _userTaskAccessRepoMock;
    private readonly DeleteTaskAccessesByUserCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskAccessesByUserCommandHandlerTests"/> class.
    /// </summary>
    public DeleteTaskAccessesByUserCommandHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._userTaskAccessRepoMock = new Mock<IUserTaskAccessRepository>();

        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses).Returns(this._userTaskAccessRepoMock.Object);

        this._handler = new DeleteTaskAccessesByUserCommandHandler(this._unitOfWorkMock.Object);
    }

    /// <summary>
    /// Ensures the handler returns a failure result when there are no shared accesses for the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNoAccessesExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteTaskAccessesByUserCommand(userId);

        this._userTaskAccessRepoMock
            .Setup(r => r.ExistsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.InvalidOperation, result.Error!.Code);
        Assert.Equal(UserTaskAccessPolicy.NoUserAccessesMessage, result.Error.Message);
    }

    /// <summary>
    /// Ensures the handler deletes all user-task accesses successfully when validation passes and accesses exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteAllAccesses_WhenValidationPassesAndAccessesExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteTaskAccessesByUserCommand(userId);

        this._userTaskAccessRepoMock
            .Setup(r => r.ExistsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        this._userTaskAccessRepoMock
            .Setup(r => r.DeleteAllByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        this._unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        this._userTaskAccessRepoMock.Verify(r => r.DeleteAllByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        this._unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
