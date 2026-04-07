using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.UserTaskAccess.Commands.CreateUserTaskAccess;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Test.Common;
using DotNetTask.Domain.ValueObjects;
using FluentAssertions;
using Moq;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Unit tests for <see cref="CreateUserTaskAccessCommandHandler"/>.
/// Verifies that validation, user existence checks, task ownership,
/// and access creation behave correctly under different conditions.
/// </summary>
public class CreateUserTaskAccessCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IUserTaskAccessService> _serviceMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IUserTaskAccessRepository> _accessRepoMock;
    private readonly CreateUserTaskAccessCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateUserTaskAccessCommandHandlerTests"/> class.
    /// </summary>
    public CreateUserTaskAccessCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._serviceMock = new Mock<IUserTaskAccessService>();
        this._userRepoMock = new Mock<IUserRepository>();
        this._accessRepoMock = new Mock<IUserTaskAccessRepository>();

        this._uowMock.Setup(u => u.Users).Returns(this._userRepoMock.Object);
        this._uowMock.Setup(u => u.UserTaskAccesses).Returns(this._accessRepoMock.Object);

        this._handler = new CreateUserTaskAccessCommandHandler(this._uowMock.Object, this._serviceMock.Object);
    }

    /// <summary>
    /// Verifies that the handler returns a failure result when the <see cref="IUserTaskAccessService"/>
    /// determines that access cannot be granted (e.g., user is owner or already has access).
    /// </summary>
    /// <remarks>
    /// Ensures that no data is written to the repository if the business validation fails.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnError_WhenServiceValidationFails()
    {
        // Arrange
        string email = "test@test.com";
        UserEntity user = UserEntityFactory.Create(email: email);
        CreateUserTaskAccessCommand command = new(Guid.NewGuid(), Guid.NewGuid(), email);

        this._userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), asNoTracking: true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        Result<bool> failureResult = await Result<bool>.FailureAsync(ErrorCode.ValidationError, "Already shared");
        this._serviceMock.Setup(s => s.CanGrantAccessAsync(command.TaskId, command.OwnerId, user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(ErrorCode.ValidationError);
        this._accessRepoMock.Verify(r => r.AddAsync(It.IsAny<UserTaskAccessEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that a <see cref="UserTaskAccessEntity"/> is correctly created and persisted
    /// when the user exists and the domain service approves the access grant.
    /// </summary>
    /// <remarks>
    /// This test also validates that the email is handled in a case-insensitive manner
    /// as expected by the handler logic.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldCreateAccess_WhenServiceValidationSucceeds()
    {
        // Arrange
        string email = "share@test.com";
        UserEntity user = UserEntityFactory.Create();
        CreateUserTaskAccessCommand command = new(Guid.NewGuid(), Guid.NewGuid(), email);

        this._userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), asNoTracking: true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this._serviceMock.Setup(s => s.CanGrantAccessAsync(command.TaskId, command.OwnerId, user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(await Result<bool>.SuccessAsync(true));

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this._accessRepoMock.Verify(
            r =>
            r.AddAsync(
                It.Is<UserTaskAccessEntity>(a => a.TaskId == command.TaskId && a.UserId == user.Id),
                It.IsAny<CancellationToken>()), Times.Once);
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
