using FluentAssertions;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Common.Services;

namespace TodoListApp.Application.Tests.Services;

/// <summary>
/// Unit tests for <see cref="TaskAccessService"/>.
/// Validates task access logic for owners and shared users.
/// </summary>
public class TaskAccessServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly TaskAccessService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskAccessServiceTests"/> class.
    /// Initializes mocks and the service under test.
    /// </summary>
    public TaskAccessServiceTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._service = new TaskAccessService(this._unitOfWorkMock.Object);
    }

    /// <summary>
    /// Verifies that <see cref="TaskAccessService.HasAccessAsync"/>
    /// returns true when the user is the owner of the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HasAccess_ResturnsTrue_WhenUserIsOwner()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        this._unitOfWorkMock.Setup(x => x.Tasks.IsTaskOwnerAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await this._service.HasAccessAsync(taskId, userId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="TaskAccessService.HasAccessAsync"/>
    /// returns true when the user is not the owner but has shared access to the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HasAccessAsync_ReturnsTrue_WhenUserHasSharedAccess()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        this._unitOfWorkMock.Setup(x => x.Tasks.IsTaskOwnerAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this._unitOfWorkMock.Setup(x => x.UserTaskAccesses.ExistsAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await this._service.HasAccessAsync(taskId, userId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="TaskAccessService.HasAccessAsync"/>
    /// returns false when the user is neither the owner nor has shared access.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HasAccessAsync_ReturnsTrue_WhenUserHasNoAccess()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        this._unitOfWorkMock.Setup(x => x.Tasks.IsTaskOwnerAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this._unitOfWorkMock.Setup(x => x.UserTaskAccesses.ExistsAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await this._service.HasAccessAsync(taskId, userId, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }
}
