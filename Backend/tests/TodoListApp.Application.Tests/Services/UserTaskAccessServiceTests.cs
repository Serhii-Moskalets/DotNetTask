using FluentAssertions;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Common.Services;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Services;

/// <summary>
/// Unit tests for <see cref="UserTaskAccessService"/>.
/// Validates the behavior of <see cref="UserTaskAccessService.CanGrantAccessAsync"/>
/// under various scenarios including null users, ownership checks, and existing access.
/// </summary>
public class UserTaskAccessServiceTests
{
    private static readonly TaskTitle Title = TaskTitle.Create("Title");
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UserTaskAccessService _service;
    private readonly string _passwordHash = new('a', 60);

    /// <summary>
    /// Initializes a new instance of the <see cref="UserTaskAccessServiceTests"/> class.
    /// Sets up the mocked <see cref="IUnitOfWork"/> and the service under test.
    /// </summary>
    public UserTaskAccessServiceTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._service = new UserTaskAccessService(this._unitOfWorkMock.Object);
    }

    /// <summary>
    /// Verifies that <see cref="UserTaskAccessService.CanGrantAccessAsync"/>
    /// returns a failure result when the shared user is <c>null</c>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
    [Fact]
    public async Task CanGrantAccessAsync_ReturnsFailure_WhenSharedUserIsNull()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        UserEntity? sharedUser = null;

        // Act
        var result = await this._service.CanGrantAccessAsync(taskId, ownerId, sharedUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.ValidationError);
        result.Error.Message.Should().Contain("Cannot grant access to this task.");
    }

    /// <summary>
    /// Verifies that <see cref="UserTaskAccessService.CanGrantAccessAsync"/>
    /// returns a failure when the specified task does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
    [Fact]
    public async Task CanGrantAccessAsync_ReturnsFailure_WhenTaskDoesNotExist()
    {
        var taskId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var sharedUser = new UserEntity("John", "john", "john@example.com", this._passwordHash);

        this._unitOfWorkMock.Setup(u => u.Tasks.GetByIdAsync(taskId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskEntity?)null);

        // Act
        var result = await this._service.CanGrantAccessAsync(taskId, ownerId, sharedUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(ErrorCode.ValidationError);
    }

    /// <summary>
    /// Verifies that access is denied if the owner ID does not match the task owner.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
    [Fact]
    public async Task CanGrantAccessAsync_ReturnsFailure_WhenOwnerIsNotCurrentUser()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var sharedUser = new UserEntity("John", "john", "john@example.com", this._passwordHash);
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), Title);

        this._unitOfWorkMock.Setup(u => u.Tasks.GetByIdAsync(taskId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        // Act
        var result = await this._service.CanGrantAccessAsync(taskId, ownerId, sharedUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(ErrorCode.ValidationError);
    }

    /// <summary>
    /// Verifies that a task cannot be shared with its owner.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
    [Fact]
    public async Task CanGrantAccessAsync_ReturnsFailure_WhenSharedUserIsOwner()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var sharedUser = new UserEntity("John", "john", "john@example.com", this._passwordHash);
        var ownerId = sharedUser.Id;
        var task = new TaskEntity(ownerId, Guid.NewGuid(), Title);

        this._unitOfWorkMock.Setup(u => u.Tasks.GetByIdAsync(taskId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        // Act
        var result = await this._service.CanGrantAccessAsync(taskId, ownerId, sharedUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(ErrorCode.ValidationError);
        result.Error.Message.Should().Contain("cannot be shared with its owner");
    }

    /// <summary>
    /// Verifies that access cannot be granted if the user already has access.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
    [Fact]
    public async Task CanGrantAccessAsync_ReturnsFailure_WhenUserAlreadyHasAccess()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var sharedUser = new UserEntity("John", "john", "john@example.com", this._passwordHash);
        var task = new TaskEntity(ownerId, Guid.NewGuid(), Title);

        this._unitOfWorkMock.Setup(u => u.Tasks.GetByIdAsync(taskId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses.ExistsAsync(taskId, sharedUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await this._service.CanGrantAccessAsync(taskId, ownerId, sharedUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(ErrorCode.InvalidOperation);
        result.Error.Message.Should().Contain("already shared with this user");
    }

    /// <summary>
    /// Verifies that access is successfully granted when all checks pass.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
    [Fact]
    public async Task CanGrantAccessAsync_ReturnsSuccess_WhenAllChecksPass()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var sharedUser = new UserEntity("John", "john", "john@example.com", this._passwordHash);
        var task = new TaskEntity(ownerId, Guid.NewGuid(), Title);

        this._unitOfWorkMock.Setup(u => u.Tasks.GetByIdAsync(taskId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses.ExistsAsync(taskId, sharedUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await this._service.CanGrantAccessAsync(taskId, ownerId, sharedUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
