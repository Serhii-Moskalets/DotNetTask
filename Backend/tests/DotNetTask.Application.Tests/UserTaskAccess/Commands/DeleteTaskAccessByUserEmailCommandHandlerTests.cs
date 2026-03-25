using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.UserTaskAccess.Commands.DeleteTaskAccessByUserEmail;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Test.Common;
using DotNetTask.Domain.ValueObjects;

using Moq;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTaskAccessByUserEmailCommandHandler"/>.
/// Verifies behavior for deleting a user-task access entry based on task ID and user email.
/// </summary>
public class DeleteTaskAccessByUserEmailCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IUserTaskAccessRepository> _utaRepository;
    private readonly DeleteTaskAccessByUserEmailCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskAccessByUserEmailCommandHandlerTests"/> class.
    /// </summary>
    public DeleteTaskAccessByUserEmailCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._utaRepository = new Mock<IUserTaskAccessRepository>();

        this._uowMock.Setup(u => u.UserTaskAccesses).Returns(this._utaRepository.Object);

        this._handler = new DeleteTaskAccessByUserEmailCommandHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Ensures the handler returns a validation error when the user does not have access to the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnValidationError_WhenUserDoesNotHaveAccess()
    {
        // Arrange
        DeleteTaskAccessByUserEmailCommand command = new DeleteTaskAccessByUserEmailCommand(Guid.NewGuid(), Guid.NewGuid(), "test@test.com");

        this._uowMock.Setup(u => u.Tasks.IsTaskOwnerAsync(command.TaskId, command.OwnerId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(false);

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.ValidationError, result.Error!.Code);
        Assert.Equal(UserTaskAccessPolicy.AccessDeniedMessage, result.Error.Message);
    }

    /// <summary>
    /// Ensures the handler returns a failure result when the user is not found.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        DeleteTaskAccessByUserEmailCommand command = new DeleteTaskAccessByUserEmailCommand(Guid.NewGuid(), Guid.NewGuid(), "test@test.com");

        this._uowMock.Setup(u => u.Tasks.IsTaskOwnerAsync(command.TaskId, command.OwnerId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);

        this._uowMock.Setup(u => u.Users.GetByEmailAsync(It.IsAny<Email>(), asNoTracking: true, It.IsAny<CancellationToken>()))
                       .ReturnsAsync((UserEntity?)null);

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.InvalidOperation, result.Error!.Code);
        Assert.Equal(UserTaskAccessPolicy.UserNotFoundMessage, result.Error.Message);
    }

    /// <summary>
    /// Ensures the handler returns a failure result when the delete operation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDeleteFails()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();
        DeleteTaskAccessByUserEmailCommand command = new DeleteTaskAccessByUserEmailCommand(Guid.NewGuid(), Guid.NewGuid(), user.Email.Value);

        this._uowMock.Setup(u => u.Tasks.IsTaskOwnerAsync(command.TaskId, command.OwnerId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);

        this._uowMock.Setup(u => u.Users.GetByEmailAsync(user.Email, asNoTracking: true, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(user);

        this._uowMock.Setup(u => u.UserTaskAccesses.DeleteByIdAsync(command.TaskId, user.Id, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(0);

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.InvalidOperation, result.Error!.Code);
        Assert.Equal(UserTaskAccessPolicy.DeleteFailedMessage, result.Error.Message);
    }

    /// <summary>
    /// Ensures the handler deletes the access successfully when all checks pass.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteAccess_WhenAllChecksPass()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();
        DeleteTaskAccessByUserEmailCommand command = new DeleteTaskAccessByUserEmailCommand(Guid.NewGuid(), Guid.NewGuid(), user.Email.Value);

        this._uowMock.Setup(u => u.Tasks.IsTaskOwnerAsync(command.TaskId, command.OwnerId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);

        this._uowMock.Setup(u => u.Users.GetByEmailAsync(user.Email, asNoTracking: true, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(user);

        this._uowMock.Setup(u => u.UserTaskAccesses.DeleteByIdAsync(command.TaskId, user.Id, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

        this._uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        this._uowMock.Verify(u => u.UserTaskAccesses.DeleteByIdAsync(command.TaskId, user.Id, It.IsAny<CancellationToken>()), Times.Once);
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}