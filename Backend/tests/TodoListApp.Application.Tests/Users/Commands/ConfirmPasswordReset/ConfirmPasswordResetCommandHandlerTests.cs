using FluentAssertions;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Common;
using TodoListApp.Application.Abstractions.Interfaces.Security;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Users.Commands.ConfirmPasswordReset;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Enums;
using TodoListApp.Domain.Exceptions;
using TodoListApp.Domain.Test.Common;

namespace TodoListApp.Application.Tests.Users.Commands.ConfirmPasswordReset;

/// <summary>
/// Contains unit tests for the <see cref="ConfirmPasswordResetCommandHandler"/> class.
/// </summary>
public class ConfirmPasswordResetCommandHandlerTests
{
    private static readonly DateTime CurrentTime = DateTime.UtcNow;
    private static readonly string NewPasswordHashString = new('b', 64);

    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IClock> _clock;
    private readonly ConfirmPasswordResetCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmPasswordResetCommandHandlerTests"/> class.
    /// </summary>
    public ConfirmPasswordResetCommandHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._passwordHasherMock = new Mock<IPasswordHasher>();
        this._clock = new Mock<IClock>();

        this._sut = new ConfirmPasswordResetCommandHandler(
            this._unitOfWorkMock.Object,
            this._passwordHasherMock.Object,
            this._clock.Object);
    }

    /// <summary>
    /// Verifies that a valid password reset request successfully updates the user's password.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_RequestIsValid()
    {
        // Arrange
        var user = UserEntityFactory.Create();
        var command = new ConfirmPasswordResetCommand("NewPassword123!", "valid-token");

        user.RequestPasswordReset("valid-token", TimeSpan.FromHours(1), CurrentTime);

        this._unitOfWorkMock.Setup(x => x.Users.GetBySecurityTokenAsync(command.Token, UserTokenType.PasswordReset, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this._passwordHasherMock.Setup(x => x.HashPassword(command.NewPassword))
            .Returns(NewPasswordHashString);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Value.Should().Be(NewPasswordHashString);
        user.CurrentToken.Should().BeNull();

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the handler throws a <see cref="DomainException"/> when the token is invalid.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ThrowDomainException_When_TokenIsInvalid()
    {
        // Arrange
        var user = UserEntityFactory.Create();
        var command = new ConfirmPasswordResetCommand("NewPass123!", "wrong-token");

        user.RequestPasswordReset("correct-token", TimeSpan.FromHours(1), CurrentTime);

        this._unitOfWorkMock.Setup(x => x.Users.GetBySecurityTokenAsync(command.Token, UserTokenType.PasswordReset, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this._passwordHasherMock.Setup(x => x.HashPassword(command.NewPassword))
            .Returns("some-hash");

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the handler returns a NotFound error when attempting to revert an email change with a non-existent
    /// security token.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_TokenDoesNotExist()
    {
        // Arrange
        var command = new ConfirmPasswordResetCommand("NewPass123!", "unknown-token");

        this._unitOfWorkMock.Setup(x => x.Users.GetBySecurityTokenAsync(
                command.Token,
                UserTokenType.PasswordReset,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
