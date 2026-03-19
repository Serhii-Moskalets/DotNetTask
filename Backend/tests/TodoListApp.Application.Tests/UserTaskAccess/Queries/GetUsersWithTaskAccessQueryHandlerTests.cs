using FluentAssertions;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.UserTaskAccess.Queries.GetUsersWithTaskAccess;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Test.Common;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.UserTaskAccess.Queries;

/// <summary>
/// Unit tests for <see cref="GetUsersWithTaskAccessQueryHandler"/>.
/// Verifies behavior when retrieving users with shared access to a task.
/// </summary>
public class GetUsersWithTaskAccessQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITaskRepository> _tasksRepoMock;
    private readonly Mock<IUserTaskAccessRepository> _userTaskAccessRepoMock;
    private readonly GetUsersWithTaskAccessQueryHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUsersWithTaskAccessQueryHandlerTests"/> class.
    /// </summary>
    public GetUsersWithTaskAccessQueryHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._tasksRepoMock = new Mock<ITaskRepository>();
        this._userTaskAccessRepoMock = new Mock<IUserTaskAccessRepository>();

        this._unitOfWorkMock.Setup(u => u.Tasks).Returns(this._tasksRepoMock.Object);
        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses).Returns(this._userTaskAccessRepoMock.Object);

        this._handler = new GetUsersWithTaskAccessQueryHandler(this._unitOfWorkMock.Object);
    }

    /// <summary>
    /// Ensures that the handler returns a failure result when the user is not the task owner.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsNotTaskOwner()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var query = new GetUsersWithTaskAccessQuery(taskId, userId);

        this._tasksRepoMock.Setup(r => r.GetTaskByIdForUserAsync(taskId, userId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskEntity?)null);

        // Act
        var result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Be(UserTaskAccessPolicy.TaskNotFoundOrAccessDeniedMessage);
    }

    /// <summary>
    /// Ensures that the handler returns the task with its shared users when the requesting user is the task owner.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnSharedUsers_WhenUserIsTaskOwner()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var task = new TaskEntity(ownerId, Guid.NewGuid(), TaskTitle.Create("Task"));
        var query = new GetUsersWithTaskAccessQuery(task.Id, ownerId);

        this._tasksRepoMock.Setup(r => r.GetTaskByIdForUserAsync(task.Id, ownerId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var sharedUsers = new List<UserTaskAccessEntity>
        {
            new(task.Id, Guid.NewGuid()) { User = UserEntityFactory.Create() },
            new(task.Id, Guid.NewGuid()) { User = UserEntityFactory.Create("rick", "rickky", "rick@test.com") },
        };

        this._userTaskAccessRepoMock.Setup(r => r.GetUserTaskAccessByTaskIdAsync(
                task.Id,
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((sharedUsers, 2));

        // Act
        var result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(task.Id);

        result.Value.Users.TotalCount.Should().Be(2);
        result.Value.Users.Items.Should().HaveCount(2);

        result.Value.Users.Items.Should().Contain(u => u.Email == UserEntityFactory.Email);
        result.Value.Users.Items.Should().Contain(u => u.Email == "rick@test.com");
    }
}
