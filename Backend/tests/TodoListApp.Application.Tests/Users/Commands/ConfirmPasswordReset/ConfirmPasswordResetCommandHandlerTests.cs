using FluentAssertions;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Security;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Users.Commands.ConfirmPasswordReset;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Application.Tests.Users.Commands.ConfirmPasswordReset;

/// <summary>
/// Contains unit tests for the <see cref="ConfirmPasswordResetCommandHandler"/> class.
/// </summary>
public class ConfirmPasswordResetCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly ConfirmPasswordResetCommandHandler _sut;

    private readonly string _oldPasswordHashString = new('a', 64);
    private readonly string _newPasswordHashString = new('b', 64);

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmPasswordResetCommandHandlerTests"/> class.
    /// </summary>
    public ConfirmPasswordResetCommandHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._passwordHasherMock = new Mock<IPasswordHasher>();

        this._sut = new ConfirmPasswordResetCommandHandler(
            this._unitOfWorkMock.Object,
            this._passwordHasherMock.Object);
    }

    /// <summary>
    /// Verifies that a valid password reset request successfully updates the user's password.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_RequestIsValid()
    {
        // Arrange
        var command = new ConfirmPasswordResetCommand("test@example.com", "NewPassword123!", "valid-token");
        var user = new UserEntity("John", "johndoe", command.Email, this._oldPasswordHashString);

        user.RequestPasswordReset("valid-token", TimeSpan.FromHours(1));

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this._passwordHasherMock.Setup(x => x.HashPassword(command.NewPassword))
            .Returns(this._newPasswordHashString);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Value.Should().Be(this._newPasswordHashString);
        user.CurrentToken.Should().BeNull();

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the handler returns a failure when the user is not found.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UserDoesNotExist()
    {
        // Arrange
        var command = new ConfirmPasswordResetCommand("nonexistent@test.com", "Pass123!", "token");

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ErrorCode.InvalidOperation);

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the handler throws a <see cref="DomainException"/> when the token is invalid.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ThrowDomainException_When_TokenIsInvalid()
    {
        // Arrange
        var command = new ConfirmPasswordResetCommand("test@example.com", "NewPass123!", "wrong-token");
        var user = new UserEntity("John", "johndoe", command.Email, this._oldPasswordHashString);

        user.RequestPasswordReset("correct-token", TimeSpan.FromHours(1));

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this._passwordHasherMock.Setup(x => x.HashPassword(command.NewPassword))
            .Returns("some-hash");

        // Act
        var act = async () => await this._sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
