using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.UserTaskAccess.Mappers;
using TodoListApp.Application.UserTaskAccess.Queries.GetSharedTaskById;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.UserTaskAccess.Queries;

/// <summary>
/// Unit tests for <see cref="GetSharedTaskByIdQueryHandler"/>.
/// Verifies behavior when retrieving a shared task by its ID and user ID.
/// </summary>
public class GetSharedTaskByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserTaskAccessRepository> _userTaskAccessRepoMock;
    private readonly GetSharedTaskByIdQueryHandler _handler;
    private readonly string _passwordHash = new('a', 64);

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSharedTaskByIdQueryHandlerTests"/> class.
    /// </summary>
    public GetSharedTaskByIdQueryHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._userTaskAccessRepoMock = new Mock<IUserTaskAccessRepository>();

        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses).Returns(this._userTaskAccessRepoMock.Object);

        this._handler = new GetSharedTaskByIdQueryHandler(this._unitOfWorkMock.Object);
    }

    /// <summary>
    /// Ensures that the handler returns a failure result when the task does not exist for the given user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTaskNotFound()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var query = new GetSharedTaskByIdQuery(taskId, userId);

        this._userTaskAccessRepoMock
            .Setup(r => r.GetByTaskAndUserIdAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserTaskAccessEntity?)null);

        // Act
        var result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(TinyResult.Enums.ErrorCode.NotFound, result.Error!.Code);
        Assert.Equal("Task not found.", result.Error.Message);
    }

    /// <summary>
    /// Ensures that the handler returns the mapped task DTO when the task exists and is shared with the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnTaskDto_WhenTaskExists()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var taskAccess = new UserTaskAccessEntity(taskId, userId)
        {
            Task = new TaskEntity(userId, Guid.NewGuid(), "Test Task"),
            User = new UserEntity("John", "john", "john@example.com", this._passwordHash),
        };

        this._userTaskAccessRepoMock
            .Setup(r => r.GetByTaskAndUserIdAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskAccess);

        var query = new GetSharedTaskByIdQuery(taskId, userId);

        // Act
        var result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var mapped = TaskAccessForUserMapper.Map(taskAccess);
        Assert.Equal(mapped.Id, result.Value!.Id);
    }
}
