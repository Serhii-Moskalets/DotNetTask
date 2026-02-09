using FluentAssertions;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Security;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Users.Commands.RegisterUser;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Users.Commands;

/// <summary>
/// Contains unit tests for the <see cref="RegisterUserCommandHandler"/> class.
/// </summary>
public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly RegisterUserCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterUserCommandHandlerTests"/> class.
    /// </summary>
    public RegisterUserCommandHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._passwordHasherMock = new Mock<IPasswordHasher>();
        this._tokenGeneratorMock = new Mock<ITokenGenerator>();

        this._sut = new RegisterUserCommandHandler(
            this._unitOfWorkMock.Object,
            this._passwordHasherMock.Object,
            this._tokenGeneratorMock.Object);
    }

    /// <summary>
    /// Verifies that a user is successfully registered when all inputs are valid and unique.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserIsUnique()
    {
        // Arrange
        var command = new RegisterUserCommand("John", "Doe", "johndoe", "john@test.com", "Password123!");
        string passwordHash = new('a', 60);
        const string secureToken = "secure_verification_token";

        this._unitOfWorkMock.Setup(x => x.Users.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        this._unitOfWorkMock.Setup(x => x.Users.ExistsByUserNameAsync(command.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        this._passwordHasherMock.Setup(x => x.HashPassword(command.Password))
            .Returns(passwordHash);
        this._tokenGeneratorMock.Setup(x => x.GenerateSecureToken())
            .Returns(secureToken);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        this._unitOfWorkMock.Verify(x => x.Users.AddAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that registration fails with a ValidationError when the email is already in use.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
        // Arrange
        var command = new RegisterUserCommand("John", "Doe", "johndoe", "existing@test.com", "Password123!");

        this._unitOfWorkMock.Setup(x => x.Users.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ErrorCode.ValidationError);

        result.Error.Message.Should().Be("Email is already exists.");

        // Перевіряємо, що збереження НЕ відбулося
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that registration fails with a ValidationError when the username is already in use.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNameAlreadyExists()
    {
        // Arrange
        var command = new RegisterUserCommand("John", "Doe", "existing_user", "john@test.com", "Password123!");

        this._unitOfWorkMock.Setup(x => x.Users.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        this._unitOfWorkMock.Setup(x => x.Users.ExistsByUserNameAsync(command.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error!.Code.Should().Be(ErrorCode.ValidationError);
        result.Error.Message.Should().Be("User name is already exists.");

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
