using FluentAssertions;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Security;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Users.Commands.LoginUser;
using TodoListApp.Domain.Entities;

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

        this.ConfirmEmail(user);

        const string generatedToken = "valid_jwt_token";

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
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

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
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

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
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

    private void ConfirmEmail(UserEntity user)
    {
        var property = typeof(UserEntity).GetProperty(nameof(user.EmailConfirmed));
        property?.SetValue(user, true);
    }
}
