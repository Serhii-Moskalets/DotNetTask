using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Application.Abstractions.Interfaces.Security;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Users.Commands.RegisterUser;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Test.Common;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tests.Users.Commands.RegisterUser;

/// <summary>
/// Contains unit tests for the <see cref="RegisterUserCommandHandler"/> class.
/// </summary>
public class RegisterUserCommandHandlerTests
{
    private const string IpAddress = "192.168.0.1";

    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly Mock<IClock> _clock;
    private readonly RegisterUserCommandHandler _sut;

    private readonly string _passwordHash = new('a', 64);

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterUserCommandHandlerTests"/> class.
    /// </summary>
    public RegisterUserCommandHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._passwordHasherMock = new Mock<IPasswordHasher>();
        this._tokenGeneratorMock = new Mock<ITokenGenerator>();
        this._clock = new Mock<IClock>();

        this._sut = new RegisterUserCommandHandler(
            this._unitOfWorkMock.Object,
            this._passwordHasherMock.Object,
            this._tokenGeneratorMock.Object,
            this._clock.Object);
    }

    /// <summary>
    /// Verifies that a new user is created and added to the repository when no user with the given email exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldCreateNewUser_WhenUserDoesNotExist()
    {
        // Arrange
        RegisterUserCommand command = new("John", "Doe", "johndoe", "new@test.com", "Password123!", IpAddress);

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(It.IsAny<Email>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        this._unitOfWorkMock.Setup(x => x.Users.ExistsByUserNameAsync(It.IsAny<UserName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this._passwordHasherMock.Setup(x => x.HashPassword(It.IsAny<string>())).Returns(this._passwordHash);
        this._tokenGeneratorMock.Setup(x => x.GenerateSecureToken()).Returns("token-123");

        // Act
        Result<Guid> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this._unitOfWorkMock.Verify(x => x.Users.AddAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that registration fails when the email exists and is already confirmed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmailIsAlreadyExistAndConfirmed()
    {
        // Arrange
        RegisterUserCommand command = new("John", "Doe", "johndoe", "existing@test.com", "Password123!", IpAddress);
        UserEntity user = UserEntityFactory.Create();

        typeof(UserEntity).GetProperty(nameof(UserEntity.EmailConfirmed))!.SetValue(user, true);

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(It.IsAny<Email>(), asNoTracking: false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        Result<Guid> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ErrorCode.InvalidOperation);

        result.Error.Message.Should().Be(EmailPolicy.AlreadyInUseMessage);

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that registration fails when the username is taken by a different user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNameTakenByAnotherUser()
    {
        // Arrange
        RegisterUserCommand command = new("John", "Doe", "existing_user", "john@test.com", "Password123!", IpAddress);

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(It.IsAny<Email>(), asNoTracking: false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);
        this._unitOfWorkMock.Setup(x => x.Users.ExistsByUserNameAsync(It.IsAny<UserName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Result<Guid> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error!.Code.Should().Be(ErrorCode.InvalidOperation);
        result.Error.Message.Should().Be(UserNamePolicy.AlreadyInUseMessage);

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that an existing unconfirmed user is updated with new details instead of creating a duplicate.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldUpdateExistingUser_WhenEmailNotConfirmed()
    {
        // Arrange
        RegisterUserCommand command = new("NewName", LastName: null, "NewUsername", "newemail@exam0ple.com", this._passwordHash, IpAddress);
        UserEntity existingUser = UserEntityFactory.Create();

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(It.IsAny<Email>(), asNoTracking: false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        this._unitOfWorkMock.Setup(x => x.Users.ExistsByUserNameAsync(It.IsAny<UserName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this._passwordHasherMock.Setup(x => x.HashPassword(It.IsAny<string>())).Returns(this._passwordHash);
        this._tokenGeneratorMock.Setup(x => x.GenerateSecureToken()).Returns("token-123");

        // Act
        Result<Guid> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingUser.FirstName.Value.Should().Be("NewName");
        existingUser.UserName.Value.Should().Be("NewUsername");
        existingUser.CurrentToken.Should().NotBeNull();
        existingUser.CurrentToken!.Value.Should().Be("token-123");
        this._unitOfWorkMock.Verify(x => x.Users.AddAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that registration fails when a user with an unconfirmed email exists,
    /// but they attempt to change their username to one that is already taken by another user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmailExistsUnconfirmedButUsernameTakenByAnotherUser()
    {
        // Arrange
        RegisterUserCommand command = new("John", null, "taken_by_other", "existing@test.com", "Password123!", IpAddress);
        UserEntity existingUser = UserEntityFactory.Create();

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(It.IsAny<Email>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        this._unitOfWorkMock.Setup(x => x.Users.ExistsByUserNameAsync(It.IsAny<UserName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Result<Guid> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ErrorCode.InvalidOperation);
        result.Error.Message.Should().Be(UserNamePolicy.AlreadyInUseMessage);
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the handler allows a retry with the same username if it belongs to the same unconfirmed user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldUpdateUser_WhenEmailExistsUnconfirmedAndUsernameIsSame()
    {
        // Arrange
        RegisterUserCommand command = new("NewName", null, "olduser", "existing@test.com", "Password123!", IpAddress);
        UserEntity existingUser = UserEntityFactory.Create("OldName", "olduser", "existing@test.com");

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(It.IsAny<Email>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        this._unitOfWorkMock.Setup(x => x.Users.ExistsByUserNameAsync(UserName.Create("olduser"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        string newPasswordHash = new('b', 64);
        this._passwordHasherMock.Setup(x => x.HashPassword(It.IsAny<string>())).Returns(newPasswordHash);
        this._tokenGeneratorMock.Setup(x => x.GenerateSecureToken()).Returns("new_token");

        // Act
        Result<Guid> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(existingUser.Id);

        existingUser.FirstName.Value.Should().Be("NewName");
        existingUser.UserName.Value.Should().Be("olduser");
        existingUser.PasswordHash.Value.Should().Be(newPasswordHash);
        existingUser.CurrentToken.Should().NotBeNull();

        this._unitOfWorkMock.Verify(x => x.Users.AddAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        this._passwordHasherMock.Verify(x => x.HashPassword(command.Password), Times.Once);
        this._tokenGeneratorMock.Verify(x => x.GenerateSecureToken(), Times.Once);
    }
}
