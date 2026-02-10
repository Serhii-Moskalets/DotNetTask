using FluentAssertions;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Users.Commands.UpdateUsername;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Users.Commands.UpdateUsername;

/// <summary>
/// Contains unit tests for the <see cref="UpdateUsernameCommandHandler"/> class.
/// </summary>
public class UpdateUsernameCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateUsernameCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUsernameCommandHandlerTests"/> class.
    /// </summary>
    public UpdateUsernameCommandHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._sut = new UpdateUsernameCommandHandler(this._unitOfWorkMock.Object);
    }

    /// <summary>
    /// Verifies that the handler returns a success result and updates the username
    /// when a valid new username is provided and it is not already taken.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldREturnSuccess_WhenUseranmeIsUpdatedSuccessfully()
    {
        // Arrange
        var user = new UserEntity("John", "john", "john@example.com", new('a', 64));
        var command = new UpdateUsernameCommand("NewUserName", user.Id);

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(user.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this._unitOfWorkMock.Setup(x => x.Users.ExistsByUserNameAsync("NewUserName", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.UserName.Value.Should().Be("NewUserName");
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the handler returns a failure result with a NotFound error
    /// when the specified user ID does not exist in the system.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new UpdateUsernameCommand("AnyName", Guid.NewGuid());
        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Be("User not found.");
    }

    /// <summary>
    /// Verifies that the handler returns a success result without performing
    /// any database updates or uniqueness checks when the new username
    /// is identical to the current one.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUsernameIsSameAsCurrent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentName = "SameName";
        var command = new UpdateUsernameCommand(currentName, userId);
        var user = new UserEntity("John", currentName, "john@test.com", new('a', 64), "Doe");

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this._unitOfWorkMock.Verify(x => x.Users.ExistsByUserNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the handler returns a failure result with a ValidationError
    /// when the requested username is already registered by another user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUsernameIsAlreadyTaken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUsernameCommand("TakenName", userId);
        var user = new UserEntity("John", "CurrentName", "john@test.com", new('a', 64), "Doe");

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this._unitOfWorkMock.Setup(x => x.Users.ExistsByUserNameAsync("TakenName", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ErrorCode.ValidationError);
        result.Error.Message.Should().Be("Username is already taken.");
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
