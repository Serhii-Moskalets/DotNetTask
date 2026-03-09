using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.UserTaskAccess.Queries.GetUsersWithTaskAccess;
using TodoListApp.Domain.Entities;
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
    private readonly string _passwordHash = new('a', 64);

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
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Task not found or you do not have permission.", result.Error.Message);
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
            new(task.Id, Guid.NewGuid()) { User = new UserEntity("John1", "john1", "john1@example.com", this._passwordHash) },
            new(task.Id, Guid.NewGuid()) { User = new UserEntity("John2", "john2", "john2@example.com", this._passwordHash) },
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
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(task.Id, result.Value!.Id);

        Assert.Equal(2, result.Value.Users.TotalCount);
        Assert.Equal(2, result.Value.Users.Items.Count);

        Assert.Contains(result.Value.Users.Items, u => u.Email == "john1@example.com");
        Assert.Contains(result.Value.Users.Items, u => u.Email == "john2@example.com");
    }
}
