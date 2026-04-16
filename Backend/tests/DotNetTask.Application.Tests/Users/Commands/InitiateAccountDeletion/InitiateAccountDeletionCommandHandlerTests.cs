using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Users.Commands.InitiateAccountDeletion;
using DotNetTask.Domain.Common;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Enums;
using DotNetTask.Domain.Test.Common;
using FluentAssertions;
using Moq;
using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tests.Users.Commands.InitiateAccountDeletion;

/// <summary>
/// Contains unit tests for the <see cref="InitiateAccountDeletionCommandHandler"/> to ensure
/// the account deletion flow is correctly handled under various conditions.
/// </summary>
public class InitiateAccountDeletionCommandHandlerTests : BaseTest
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly InitiateAccountDeletionCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="InitiateAccountDeletionCommandHandlerTests"/> class.
    /// </summary>
    public InitiateAccountDeletionCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._sut = new InitiateAccountDeletionCommandHandler(this._uowMock.Object, this.Clock);
    }

    /// <summary>
    /// Verifies that the handler returns a failure result and does not persist changes
    /// when the specified user does not exist in the database.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenUserDoesNotExist()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        InitiateAccountDeletionCommand command = new(userId);

        this._uowMock.Setup(x => x.Users.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        Result<Unit> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Be(UserPolicy.AccountNotFoundMessage);

        this._uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the handler returns a failure result when the user is found
    /// but their current status (e.g., Unconfirmed) prohibits initiating account deletion.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenUserIsNotActive()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();

        InitiateAccountDeletionCommand command = new(user.Id);

        this._uowMock.Setup(x => x.Users.GetByIdAsync(user.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        Result<Unit> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be(UserPolicy.EmailIsNotConfirmedMessage);

        this._uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the handler successfully updates the user status, schedules the deletion,
    /// and persists the changes when a valid active user requests account removal.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnSuccess_AndSaveChange_WhenDataIsValid()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();

        DateTime currentTime = this.Clock.UtcNow;

        InitiateAccountDeletionCommand command = new(user.Id);

        this._uowMock.Setup(x => x.Users.GetByIdAsync(user.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        Result<Unit> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Status.Should().Be(UserStatus.PendingDeletion);
        user.DeletionScheduledAt.Should().Be(currentTime.AddDays(UserPolicy.DeletionDelayInDays));

        this._uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
