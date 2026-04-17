using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Users.Commands.RecoverAccount;
using DotNetTask.Domain.Common;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Enums;
using DotNetTask.Domain.Test.Common;
using FluentAssertions;
using Moq;
using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tests.Users.Commands.RecoverAccount;

/// <summary>
/// Provides unit tests for the <see cref="RecoverAccountCommandHandler"/> to validate the
/// process of restoring user accounts from a pending deletion state.
/// </summary>
public class RecoverAccountCommandHandlerTests : BaseTest
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly RecoverAccountCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverAccountCommandHandlerTests"/> class.
    /// </summary>
    public RecoverAccountCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._sut = new RecoverAccountCommandHandler(this._uowMock.Object, this.Clock);
    }

    /// <summary>
    /// Verifies that the handler returns a failure result when the user ID provided
    /// in the command does not match any existing user in the repository.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenUserDoesNotExist()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RecoverAccountCommand command = new(userId);

        this._uowMock.Setup(x => x.Users.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        Result<Unit> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);

        this._uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the handler returns a failure result if the account recovery is attempted
    /// after the grace period (deletion delay) has already passed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenDeletionPeriodHasExpired()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();

        DateTime expiredDeletionDate = this.Clock.UtcNow.AddMinutes(-1);

        user.RequestAccountDeletion(this.Clock.UtcNow.AddDays(-31));

        RecoverAccountCommand command = new(user.Id);
        this._uowMock.Setup(x => x.Users.GetByIdAsync(user.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        Result<Unit> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be(UserPolicy.DeletionPeriodExpiredMessage);

        this._uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the handler successfully restores the account to active status
    /// and clears the scheduled deletion date when the request is made within the valid period.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenAccountIsRestoredWithinPeriod()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        UserEntity user = UserEntityFactory.CreateActive();

        user.RequestAccountDeletion(this.Clock.UtcNow);

        RecoverAccountCommand command = new(userId);
        this._uowMock.Setup(x => x.Users.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        Result<Unit> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Active);
        user.DeletionScheduledAt.Should().BeNull();

        this._uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

}
