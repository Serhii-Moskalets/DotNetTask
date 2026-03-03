using FluentAssertions;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Security;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Users.Commands.UpdatePassword;
using TodoListApp.Domain.Entities;
using TodoListApp.Infrastructure.Persistence.UnitOfWork;

namespace TodoListApp.Application.Tests.Users.Commands.UpdatePassword;

/// <summary>
/// Contains unit tests for the <see cref="UpdatePasswordCommandHandler"/> class.
/// </summary>
public class UpdatePasswordCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly UpdatePasswordCommandHandler _sut;

    private readonly string _newPaswordHashString = new('b', 64);
    private readonly string _paswordHashString = new('a', 64);

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePasswordCommandHandlerTests"/> class.
    /// </summary>
    public UpdatePasswordCommandHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._passwordHasherMock = new Mock<IPasswordHasher>();
        this._sut = new UpdatePasswordCommandHandler(
            this._unitOfWorkMock.Object,
            this._passwordHasherMock.Object);
    }

    /// <summary>
    /// Verifies that the handler returns a success result and updates the password
    /// when the user exists and the current password is correct.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCurrentPasswordIsCorrect()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdatePasswordCommand("OldPass123!", "NewPass123!", userId);
        var user = new UserEntity("John", "johnny", "john@test.com", this._paswordHashString);

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this._passwordHasherMock.Setup(x => x.VerifyPassword(command.CurrentPassword, user.PasswordHash.Value))
            .Returns(true);

        this._passwordHasherMock.Setup(x => x.HashPassword(command.NewPassword))
            .Returns(this._newPaswordHashString);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Value.Should().Be(this._newPaswordHashString);
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the handler returns a failure result with a NotFound error
    /// when the user does not exist in the system.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdatePasswordCommand("OldPass!", "NewPass!", userId);

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the handler returns a failure result with a ValidationError
    /// when the current password provided by the user is incorrect.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCurrentPasswordIsIncorrect()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdatePasswordCommand("WrongPass!", "NewPass!", userId);
        var user = new UserEntity("John", "johnny", "john@test.com", this._paswordHashString);

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this._passwordHasherMock.Setup(x => x.VerifyPassword(command.CurrentPassword, It.IsAny<string>()))
            .Returns(false);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(ErrorCode.ValidationError);
        result.Error.Message.Should().Be("Incorrect password.");
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
