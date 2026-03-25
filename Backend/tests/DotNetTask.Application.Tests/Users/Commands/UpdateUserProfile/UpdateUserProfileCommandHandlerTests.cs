using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Users.Commands.UpdateUserProfile;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Test.Common;

using FluentAssertions;

using Moq;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tests.Users.Commands.UpdateUserProfile;

/// <summary>
/// Contains unit tests for the <see cref="UpdateUserProfileCommandHandler"/> class.
/// </summary>
public class UpdateUserProfileCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateUserProfileCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserProfileCommandHandlerTests"/> class.
    /// </summary>
    public UpdateUserProfileCommandHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._sut = new UpdateUserProfileCommandHandler(this._unitOfWorkMock.Object);
    }

    /// <summary>
    /// Verifies that the handler returns a success result and updates the user's first name
    /// when a valid new first name is provided.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenFirstNameIsChanged()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();
        UpdateUserProfileCommand command = new UpdateUserProfileCommand("NewName", null, user.Id);

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(user.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        Result<bool> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.FirstName.Value.Should().Be("NewName");
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the handler returns a failure result with an InvalidOperation error
    /// and does not perform a database update when the provided names are identical to the current ones.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNoChangesDetected()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();
        UpdateUserProfileCommand command = new UpdateUserProfileCommand(UserEntityFactory.FirstName, UserEntityFactory.LastName, user.Id);

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(user.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        Result<bool> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(ErrorCode.InvalidOperation);
        result.Error.Message.Should().Be(UserPolicy.NoChangesDetectedMessage);

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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
        Guid userId = Guid.NewGuid();
        UpdateUserProfileCommand command = new UpdateUserProfileCommand("Name", "LastName", userId);

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        Result<bool> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
    }

    /// <summary>
    /// Verifies that the handler updates only the last name and leaves the first name unchanged
    /// when the command provides a new last name but a null first name.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldOnlyUpdateLastName_WhenFirstNameIsNullInCommand()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        UpdateUserProfileCommand command = new UpdateUserProfileCommand(null, "NewLastName", userId);
        UserEntity user = UserEntityFactory.Create();

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await this._sut.Handle(command, CancellationToken.None);

        // Assert
        user.FirstName.Value.Should().Be(UserEntityFactory.FirstName);
        user.LastName!.Value.Should().Be("NewLastName");
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
