using FluentAssertions;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Security;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Users.Commands.LoginUser;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Users.Commands.LoginUser;

/// <summary>
/// Contains unit tests for the <see cref="LoginUserCommandHandler"/> class.
/// </summary>
public class LoginUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly LoginUserCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginUserCommandHandlerTests"/> class.
    /// </summary>
    public LoginUserCommandHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._passwordHasherMock = new Mock<IPasswordHasher>();
        this._jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();

        this._sut = new LoginUserCommandHandler(
            this._unitOfWorkMock.Object,
            this._passwordHasherMock.Object,
            this._jwtTokenGeneratorMock.Object);
    }

    /// <summary>
    /// Verifies that a valid user is successfully authenticated and receives a token.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        // Arrange
        var command = new LoginUserCommand("john@test.com", "CorrectPassword123!");
        var user = new UserEntity("John", "johndoe", command.Email, new('a', 64), "Doe");

        ConfirmEmail(user);

        const string generatedToken = "valid_jwt_token";

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(It.IsAny<Email>(), asNoTracking: true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this._passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash.Value))
            .Returns(true);

        this._jwtTokenGeneratorMock.Setup(x => x.GenerateToken(user))
            .Returns(generatedToken);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be(generatedToken);
        result.Value.Email.Should().Be(command.Email);

        this._jwtTokenGeneratorMock.Verify(x => x.GenerateToken(user), Times.Once);
    }

    /// <summary>
    /// Verifies that login fails with a generic error when the user does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new LoginUserCommand("nonexistent@test.com", "any_password");

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(It.IsAny<Email>(), asNoTracking: true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ErrorCode.ValidationError);
        result.Error.Message.Should().Be("Invalid email or password.");

        this._jwtTokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<UserEntity>()), Times.Never);
    }

    /// <summary>
    /// Verifies that login fails with the same generic error when the password is incorrect.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPasswordIsIncorrect()
    {
        // Arrange
        var command = new LoginUserCommand("john@test.com", "WrongPassword!");
        var user = new UserEntity("John", "johndoe", command.Email, new('a', 64), "Doe");

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(It.IsAny<Email>(), asNoTracking: true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this._passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash.Value))
            .Returns(false);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ErrorCode.ValidationError);
        result.Error.Message.Should().Be("Invalid email or password.");

        this._jwtTokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<UserEntity>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the login process returns a success result indicating that a password change
    /// is required, and does not generate a JWT, when the <see cref="UserEntity.MustChangePassword"/>
    /// flag is set to <see langword="true"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnMustChangePassword_WhenFlagIsTrue()
    {
        // Arrange
        var command = new LoginUserCommand("john@test.com", "Password123!");
        var user = new UserEntity("John", "johndoe", command.Email, new('a', 64));

        ConfirmEmail(user);

        var revertToken = "revert";
        user.RequestEmailChange(Email.Create("new@test.com"), "token", revertToken, TimeSpan.FromHours(1));
        user.ConfirmEmailChange("token", DateTime.UtcNow);
        user.RevertEmailChange(revertToken, DateTime.UtcNow);

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(It.IsAny<Email>(), asNoTracking: true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this._passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash.Value))
            .Returns(true);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.MustChangePassword.Should().BeTrue();
        result.Value.Token.Should().BeNull();
        this._jwtTokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<UserEntity>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the login process fails with a validation error when the user
    /// has not yet confirmed their email address.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmailIsNotConfirmed()
    {
        // Arrange
        var command = new LoginUserCommand("john@test.com", "Password123!");
        var user = new UserEntity("John", "johndoe", command.Email, new('a', 64));

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(It.IsAny<Email>(), asNoTracking: true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this._passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash.Value))
            .Returns(true);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("Please confirm your email before logging in.");
    }

    private static void ConfirmEmail(UserEntity user)
    {
        var token = "any-token";
        user.RequestEmailVerification(token, TimeSpan.FromHours(1));
        user.ConfirmEmailVerification(token, DateTime.UtcNow);
    }
}
