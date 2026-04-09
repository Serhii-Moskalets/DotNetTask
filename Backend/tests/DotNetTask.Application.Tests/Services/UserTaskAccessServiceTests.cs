using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Common.Services;
using DotNetTask.Domain.Common;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Test.Common;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tests.Services;

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
        Guid taskId = Guid.NewGuid();
        Guid ownerId = Guid.NewGuid();
        UserEntity? sharedUser = null;

        // Act
        Result<Unit> result = await this._service.CanGrantAccessAsync(taskId, ownerId, sharedUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.ValidationError);
        result.Error.Message.Should().Be(UserTaskAccessPolicy.UserNotFoundMessage);
    }

    /// <summary>
    /// Verifies that <see cref="UserTaskAccessService.CanGrantAccessAsync"/>
    /// returns a failure when the specified task does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
    [Fact]
    public async Task CanGrantAccessAsync_ReturnsFailure_WhenTaskDoesNotExist()
    {
        Guid taskId = Guid.NewGuid();
        Guid ownerId = Guid.NewGuid();
        UserEntity sharedUser = UserEntityFactory.Create();

        this._unitOfWorkMock.Setup(u => u.Tasks.GetByIdAsync(taskId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskEntity?)null);

        // Act
        Result<Unit> result = await this._service.CanGrantAccessAsync(taskId, ownerId, sharedUser, CancellationToken.None);

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
        Guid taskId = Guid.NewGuid();
        Guid ownerId = Guid.NewGuid();
        UserEntity sharedUser = UserEntityFactory.Create();
        TaskEntity task = new(Guid.NewGuid(), Guid.NewGuid(), Title);

        this._unitOfWorkMock.Setup(u => u.Tasks.GetByIdAsync(taskId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        // Act
        Result<Unit> result = await this._service.CanGrantAccessAsync(taskId, ownerId, sharedUser, CancellationToken.None);

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
        Guid taskId = Guid.NewGuid();
        UserEntity sharedUser = UserEntityFactory.Create();
        Guid ownerId = sharedUser.Id;
        TaskEntity task = new(ownerId, Guid.NewGuid(), Title);

        this._unitOfWorkMock.Setup(u => u.Tasks.GetByIdAsync(taskId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        // Act
        Result<Unit> result = await this._service.CanGrantAccessAsync(taskId, ownerId, sharedUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(ErrorCode.ValidationError);
        result.Error.Message.Should().Be(UserTaskAccessPolicy.CannotShareWithOwnerMessage);
    }

    /// <summary>
    /// Verifies that access cannot be granted if the user already has access.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
    [Fact]
    public async Task CanGrantAccessAsync_ReturnsFailure_WhenUserAlreadyHasAccess()
    {
        // Arrange
        Guid ownerId = Guid.NewGuid();
        Guid taskId = Guid.NewGuid();
        UserEntity sharedUser = UserEntityFactory.Create();
        TaskEntity task = new(ownerId, Guid.NewGuid(), Title);

        this._unitOfWorkMock.Setup(u => u.Tasks.GetByIdAsync(taskId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses.ExistsAsync(taskId, sharedUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Result<Unit> result = await this._service.CanGrantAccessAsync(taskId, ownerId, sharedUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(ErrorCode.InvalidOperation);
        result.Error.Message.Should().Be(UserTaskAccessPolicy.AlreadySharedMessage);
    }

    /// <summary>
    /// Verifies that access is successfully granted when all checks pass.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
    [Fact]
    public async Task CanGrantAccessAsync_ReturnsSuccess_WhenAllChecksPass()
    {
        // Arrange
        Guid ownerId = Guid.NewGuid();
        Guid taskId = Guid.NewGuid();
        UserEntity sharedUser = UserEntityFactory.Create();
        TaskEntity task = new(ownerId, Guid.NewGuid(), Title);

        this._unitOfWorkMock.Setup(u => u.Tasks.GetByIdAsync(taskId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses.ExistsAsync(taskId, sharedUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        Result<Unit> result = await this._service.CanGrantAccessAsync(taskId, ownerId, sharedUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
