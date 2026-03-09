using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.UserTaskAccess.Queries.GetSharedTasksByUserId;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.UserTaskAccess.Queries;

/// <summary>
/// Unit tests for <see cref="GetSharedTasksByUserIdQueryHandler"/>.
/// Verifies behavior when retrieving all tasks shared with a specific user.
/// </summary>
public class GetSharedTasksByUserIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserTaskAccessRepository> _userTaskAccessRepoMock;
    private readonly GetSharedTasksByUserIdQueryHandler _handler;
    private readonly string _passwordHash = new('a', 64);

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSharedTasksByUserIdQueryHandlerTests"/> class.
    /// </summary>
    public GetSharedTasksByUserIdQueryHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._userTaskAccessRepoMock = new Mock<IUserTaskAccessRepository>();

        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses).Returns(this._userTaskAccessRepoMock.Object);

        this._handler = new GetSharedTasksByUserIdQueryHandler(this._unitOfWorkMock.Object);
    }

    /// <summary>
    /// Ensures that the handler returns an empty list when there are no shared tasks for the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnEmptyPagedResult_WhenNoSharedTasksExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetSharedTasksByUserIdQuery(userId, 1, 10);

        this._userTaskAccessRepoMock
            .Setup(r => r.GetSharedTasksByUserIdAsync(userId, query.Page, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<UserTaskAccessEntity>(), 0));

        // Act
        var result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }

    /// <summary>
    /// Ensures that the handler returns mapped task DTOs when shared tasks exist for the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnMappedPagedResult_WhenSharedTasksExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetSharedTasksByUserIdQuery(userId, 1, 10);

        var task1 = new TaskEntity(userId, Guid.NewGuid(), TaskTitle.Create("Task"));
        var entities = new List<UserTaskAccessEntity>
        {
            new(task1.Id, userId) { Task = task1, User = new UserEntity("Owner", "owner", "o@t.com", this._passwordHash) },
        };

        this._userTaskAccessRepoMock
            .Setup(r => r.GetSharedTasksByUserIdAsync(userId, query.Page, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((entities, 1));

        // Act
        var result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.Equal(query.Page, result.Value.Page);
        Assert.Equal(task1.Id, result.Value.Items.First().Id);
    }
}
