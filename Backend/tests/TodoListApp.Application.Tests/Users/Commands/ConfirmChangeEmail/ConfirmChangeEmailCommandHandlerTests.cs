using FluentAssertions;
using Moq;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.Common;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Users.Commands.ConfirmEmailChange;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Enums;
using TodoListApp.Domain.Test.Common;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Users.Commands.ConfirmChangeEmail;

/// <summary>
/// Contains unit tests for the <see cref="ConfirmEmailChangeCommandHandler"/> class.
/// </summary>
public class ConfirmEmailChangeCommandHandlerTests
{
    private const string PendingEmail = "new@example.com";
    private const string ConfirmToken = "confirm-token";
    private const string RevertToken = "revert-token";

    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IClock> _clock;
    private readonly ConfirmEmailChangeCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmEmailChangeCommandHandlerTests"/> class.
    /// </summary>
    public ConfirmEmailChangeCommandHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._clock = new Mock<IClock>();
        this._sut = new ConfirmEmailChangeCommandHandler(this._unitOfWorkMock.Object, this._clock.Object);
    }

    /// <summary>
    /// Verifies that the handler returns a failure result when the user is not found by the token.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnFail_When_UserNotFound()
    {
        // Arrange
        var command = new ConfirmEmailChangeCommand("non-existent-token");

        this._unitOfWorkMock.Setup(x => x.Users.GetBySecurityTokenAsync(
            It.IsAny<string>(),
            UserTokenType.EmailChange,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(TinyResult.Enums.ErrorCode.NotFound);

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the handler throws a DomainException when the user is found,
    /// but the token is invalid or expired (Domain Logic check).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_TokenIsInvalidForFoundUser()
    {
        // Arrange
        var command = new ConfirmEmailChangeCommand("wrong-token");
        var user = UserEntityFactory.Create();

        user.RequestEmailChange(Email.Create(PendingEmail), ConfirmToken, RevertToken, TimeSpan.FromHours(1), DateTime.UtcNow);

        this._unitOfWorkMock.Setup(x => x.Users.GetBySecurityTokenAsync(
                It.IsAny<string>(),
                UserTokenType.EmailChange,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
