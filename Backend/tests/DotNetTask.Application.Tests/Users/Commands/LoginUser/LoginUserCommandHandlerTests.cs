using DotNetTask.Application.Abstractions.Interfaces.Security;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Users.Commands.LoginUser;
using DotNetTask.Domain.Common;
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
public class LoginUserCommandHandlerTests : BaseTest
{
    private const string IpAddress = "192.168.0.1";
    private const string GeneratedToken = "valid_jwt_token";

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
    public async Task Handle_Should_ReturnSuccess_WhenEverythingIsValid()
    {
        // Arrange
        LoginUserCommand command = new("john@test.com", "CorrectPassword123!", IpAddress);
        UserEntity user = UserEntityFactory.CreateActive(email: command.Email);

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
    public async Task Handle_Should_ReturnFailure_WhenUserDoesNotExist()
    {
        // Arrange
        LoginUserCommand command = new("nonexistent@test.com", "any_password", IpAddress);

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
    public async Task Handle_Should_ReturnFailure_WhenPasswordIsIncorrect()
    {
        // Arrange
        LoginUserCommand command = new("john@test.com", "WrongPassword!", IpAddress);
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
    public async Task Handle_Should_ReturnMustChangePassword_WhenFlagIsTrue_And_EmailIsConfirmed()
    {
        // Arrange
        LoginUserCommand command = new("john@test.com", "Password123!", IpAddress);
        UserEntity user = UserEntityFactory.CreateActive();

        this.SetMustChangePassword(user);

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
        result.Value.Should().NotBeNull();
        result.Value.IsEmailConfirmed.Should().BeTrue();
        result.Value.MustChangePassword.Should().BeTrue();
        result.Value.Token.Should().NotBeNull();
        this._jwtTokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<UserEntity>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the login process returns a success result with an unconfirmed email status
    /// and a valid token when the user has not yet confirmed their email address.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnUnconfirmed_WhenUserStatusIsUnconfirmed()
    {
        // Arrange
        LoginUserCommand command = new("john@test.com", "Password123!", IpAddress);
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

    /// <summary>
    /// Verifies that the login process returns a success result with an аccount pending deletion status
    /// and a valid token when the user's account has pending deletion status.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnPendingDeletion_WhenUserStatusIsPendingDeletion()
    {
        // Arrange
        LoginUserCommand command = new("john@test.com", "Password123!", IpAddress);
        UserEntity user = UserEntityFactory.CreateActive();

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(It.IsAny<Email>(), asNoTracking: true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this._passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash.Value))
            .Returns(true);

        this._jwtTokenGeneratorMock.Setup(x => x.GenerateToken(user)).Returns(GeneratedToken);

        Result<Unit> resultAcountDeletion = user.RequestAccountDeletion(this.Clock.UtcNow);

        // Act
        Result<LoginResponse> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        resultAcountDeletion.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.MustChangePassword.Should().BeFalse();
        result.Value.IsAccountPendingDeletion.Should().BeTrue();
        result.Value.Token.Should().NotBeNull();

        this._jwtTokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<UserEntity>()), Times.Once);
    }

    private void SetMustChangePassword(UserEntity user)
    {

        string revertToken = "revert";
        user.RequestEmailChange(Email.Create("new@test.com"), "token", revertToken, TimeSpan.FromHours(1), this.Clock.UtcNow);
        user.ConfirmEmailChange("token", this.Clock.UtcNow.AddMinutes(1));
        user.RevertEmailChange(revertToken, this.Clock.UtcNow.AddMinutes(2));
    }
}
