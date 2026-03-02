using FluentAssertions;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Users.Commands.ConfirmEmail;
using TodoListApp.Application.Users.Commands.ConfirmPasswordReset;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Enums;
using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Application.Tests.Users.Commands.ConfirmEmail;

/// <summary>
/// Contains unit tests for the <see cref="ConfirmEmailCommandHandler"/> class.
/// </summary>
public class ConfirmEmailCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ConfirmEmailCommandHandler _sut;
    private readonly string _passwordHash = new('a', 64);

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmEmailCommandHandlerTests"/> class.
    /// </summary>
    public ConfirmEmailCommandHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._sut = new ConfirmEmailCommandHandler(this._unitOfWorkMock.Object);
    }

    /// <summary>
    /// Verifies that a valid token successfully confirms the email.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_TokenIsValid()
    {
        // Arrange
        const string token = "valid-token";
        var command = new ConfirmEmailCommand(token);
        var user = new UserEntity("John", "john", "test@example.com", this._passwordHash);

        user.RequestEmailVerification(token, TimeSpan.FromHours(1));

        this._unitOfWorkMock.Setup(x => x.Users.GetBySecurityTokenAsync(command.Token, UserTokenType.EmailVerification, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.EmailConfirmed.Should().BeTrue();
        user.CurrentToken.Should().BeNull();

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that a DomainException is thrown when the token is invalid or expired.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ThrowDomainException_When_TokenIsInvalid()
    {
        // Arrange
        var command = new ConfirmEmailCommand("wrong-token");
        var user = new UserEntity("John", "john", "test@example.com", this._passwordHash);

        user.RequestEmailVerification("valid-token", TimeSpan.FromHours(1));

        this._unitOfWorkMock.Setup(x => x.Users.GetBySecurityTokenAsync(command.Token, UserTokenType.EmailVerification, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var act = () => this._sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Invalid or expired email verification token.");

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
        var command = new ConfirmEmailCommand("unknown-token");

        this._unitOfWorkMock.Setup(x => x.Users.GetBySecurityTokenAsync(command.Token, UserTokenType.EmailVerification, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
