using DotNetTask.Application.Abstractions.Interfaces.Security;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Users.Commands.LoginUser;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Test.Common;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tests.Users.Commands.LoginUser;

/// <summary>
/// Contains unit tests for the <see cref="LoginUserCommandHandler"/> class.
/// </summary>
public class LoginUserCommandHandlerTests
{
    private const string GeneratedToken = "valid_jwt_token";
    private static readonly DateTime CurrentTime = DateTime.UtcNow;

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
        LoginUserCommand command = new LoginUserCommand("john@test.com", "CorrectPassword123!");
        UserEntity user = UserEntityFactory.Create(email: command.Email);

        ConfirmEmail(user);

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(It.IsAny<Email>(), asNoTracking: true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this._passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash.Value))
            .Returns(true);

        this._jwtTokenGeneratorMock.Setup(x => x.GenerateToken(user))
            .Returns(GeneratedToken);

        // Act
        Result<LoginResponse> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be(GeneratedToken);
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
        LoginUserCommand command = new LoginUserCommand("nonexistent@test.com", "any_password");

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(It.IsAny<Email>(), asNoTracking: true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        Result<LoginResponse> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ErrorCode.ValidationError);
        result.Error.Message.Should().Be(UserPolicy.InvalidCredentialsMessage);

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
        LoginUserCommand command = new LoginUserCommand("john@test.com", "WrongPassword!");
        UserEntity user = UserEntityFactory.Create();

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(It.IsAny<Email>(), asNoTracking: true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this._passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash.Value))
            .Returns(false);

        // Act
        Result<LoginResponse> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ErrorCode.ValidationError);
        result.Error.Message.Should().Be(UserPolicy.InvalidCredentialsMessage);

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
        LoginUserCommand command = new LoginUserCommand("john@test.com", "Password123!");
        UserEntity user = UserEntityFactory.Create();

        ConfirmEmail(user);

        string revertToken = "revert";
        user.RequestEmailChange(Email.Create("new@test.com"), "token", revertToken, TimeSpan.FromHours(1), CurrentTime);
        user.ConfirmEmailChange("token", DateTime.UtcNow);
        user.RevertEmailChange(revertToken, DateTime.UtcNow, "reset-token", TimeSpan.FromMinutes(15));

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(It.IsAny<Email>(), asNoTracking: true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this._passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash.Value))
            .Returns(true);

        // Act
        Result<LoginResponse> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.IsEmailConfirmed.Should().BeTrue();
        result.Value.MustChangePassword.Should().BeTrue();
        result.Value.Token.Should().BeNull();
        this._jwtTokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<UserEntity>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the login process fails with a validation error when the user
    /// has not yet confirmed their email address.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnSuccessWithUnconfirmedEmail_WhenEmailIsNotConfirmed()
    {
        // Arrange
        LoginUserCommand command = new LoginUserCommand("john@test.com", "Password123!");
        UserEntity user = UserEntityFactory.Create();

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(It.IsAny<Email>(), asNoTracking: true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this._passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash.Value))
            .Returns(true);

        this._jwtTokenGeneratorMock.Setup(x => x.GenerateToken(user)).Returns(GeneratedToken);

        // Act
        Result<LoginResponse> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.MustChangePassword.Should().BeFalse();
        result.Value.IsEmailConfirmed.Should().BeFalse();
        result.Value.Token.Should().NotBeNull();

        this._jwtTokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<UserEntity>()), Times.Once);
    }

    private static void ConfirmEmail(UserEntity user)
    {
        string token = "any-token";
        user.RequestEmailVerification(token, TimeSpan.FromHours(1), CurrentTime);
        user.ConfirmEmailVerification(token, DateTime.UtcNow);
    }
}
