using FluentAssertions;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Security;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Users.Commands.ChangeEmail;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Users.Commands.ChangeEmail;

/// <summary>
/// Contains unit tests for the <see cref="ChangeEmailCommandHandler"/> class.
/// </summary>
public class ChangeEmailCommandHandlerTests
{
    private const string NewEmail = "newemail@example.com";
    private const string OldEmail = "oldemail@example.com";
    private const string GeneratedToken = "secure-token-123";
    private readonly string passwordHashString = new('a', 64);

    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly ChangeEmailCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeEmailCommandHandlerTests"/> class.
    /// </summary>
    public ChangeEmailCommandHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._tokenGeneratorMock = new Mock<ITokenGenerator>();
        this._sut = new ChangeEmailCommandHandler(
            this._unitOfWorkMock.Object, this._tokenGeneratorMock.Object);
    }

    /// <summary>
    /// Verifies that a valid email change request successfully updates the user state and saves changes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_RequestIsValid()
    {
        // Arrange
        var command = new ChangeEmailCommand(NewEmail, Guid.NewGuid());
        var user = new UserEntity("John", "john", OldEmail, this.passwordHashString);

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(command.UserId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this._unitOfWorkMock.Setup(x => x.Users.ExistsByEmailAsync(command.NewEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this._tokenGeneratorMock.Setup(x => x.GenerateSecureToken())
            .Returns(GeneratedToken);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        user.CurrentToken.Should().NotBeNull();
        user.CurrentToken!.Value.Should().Be(GeneratedToken);
        user.CurrentToken.Metadata.Should().Be(command.NewEmail);

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the handler returns a NotFound failure when the user is not found in the database.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_UserDoesNotExist()
    {
        // Arrange
        var command = new ChangeEmailCommand(NewEmail, Guid.NewGuid());

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(command.UserId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(TinyResult.Enums.ErrorCode.NotFound);
        result.Error.Message.Should().Be("User not found.");

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    /// <summary>
    /// Verifies that the handler returns an InvalidOperation failure when the new email is already taken.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_EmailAlreadyInUse()
    {
        // Arrange
        var command = new ChangeEmailCommand(NewEmail, Guid.NewGuid());
        var user = new UserEntity("John", "john", OldEmail, this.passwordHashString);

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(command.UserId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this._unitOfWorkMock.Setup(x => x.Users.ExistsByEmailAsync(command.NewEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(TinyResult.Enums.ErrorCode.InvalidOperation);
        result.Error.Message.Should().Be("This email is already in use.");

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        this._tokenGeneratorMock.Verify(x => x.GenerateSecureToken(), Times.Never());
    }

    /// <summary>
    /// Verifies that the handler returns a ValidationError when the new email is the same as the current email.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnValidationError_When_EmailIsSameAsCurrent()
    {
        // Arrange
        var command = new ChangeEmailCommand(OldEmail, Guid.NewGuid());
        var user = new UserEntity("John", "john", OldEmail, this.passwordHashString);

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(command.UserId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(TinyResult.Enums.ErrorCode.ValidationError);
        result.Error.Message.Should().Be("New email is same as current.");

        this._unitOfWorkMock.Verify(x => x.Users.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }
}
