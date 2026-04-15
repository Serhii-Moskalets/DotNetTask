using DotNetTask.Application.Abstractions.Interfaces.Security;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Users.Commands.UpdatePassword;
using DotNetTask.Domain.Common;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Test.Common;

using FluentAssertions;

using Moq;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tests.Users.Commands.UpdatePassword;

/// <summary>
/// Contains unit tests for the <see cref="UpdatePasswordCommandHandler"/> class.
/// </summary>
public class UpdatePasswordCommandHandlerTests
{
    private static readonly string NewPaswordHashString = new('b', 64);

    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly UpdatePasswordCommandHandler _sut;

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
    public async Task Handle_Should_ReturnSuccess_WhenCurrentPasswordIsCorrect()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();
        UpdatePasswordCommand command = new("OldPass123!", "NewPass123!", user.Id);

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(user.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this._passwordHasherMock.Setup(x => x.VerifyPassword(command.CurrentPassword, user.PasswordHash.Value))
            .Returns(true);

        this._passwordHasherMock.Setup(x => x.HashPassword(command.NewPassword))
            .Returns(NewPaswordHashString);

        // Act
        Result<Unit> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Value.Should().Be(NewPaswordHashString);
        user.MustChangePassword.Should().BeFalse();
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the handler returns a failure result with a NotFound error
    /// when the user does not exist in the system.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        UpdatePasswordCommand command = new("OldPass!", "NewPass!", userId);

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        Result<Unit> result = await this._sut.Handle(command, CancellationToken.None);

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
    public async Task Handle_Should_ReturnFailure_WhenCurrentPasswordIsIncorrect()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();
        UpdatePasswordCommand command = new("WrongPass!", "NewPass!", user.Id);

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(user.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this._passwordHasherMock.Setup(x => x.VerifyPassword(command.CurrentPassword, It.IsAny<string>()))
            .Returns(false);

        // Act
        Result<Unit> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(ErrorCode.ValidationError);
        result.Error.Message.Should().Be(PasswordPolicy.IncorrectMessage);
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
