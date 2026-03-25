using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Common.Dtos;
using DotNetTask.Application.UserTaskAccess.Queries.GetSharedTasksByUserId;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Test.Common;
using DotNetTask.Domain.ValueObjects;

using Moq;

using TinyResult;

namespace DotNetTask.Application.Tests.UserTaskAccess.Queries;

/// <summary>
/// Unit tests for <see cref="GetSharedTasksByUserIdQueryHandler"/>.
/// Verifies behavior when retrieving all tasks shared with a specific user.
/// </summary>
public class GetSharedTasksByUserIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserTaskAccessRepository> _userTaskAccessRepoMock;
    private readonly GetSharedTasksByUserIdQueryHandler _handler;

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
        Guid userId = Guid.NewGuid();
        GetSharedTasksByUserIdQuery query = new GetSharedTasksByUserIdQuery(userId, 1, 10);

        this._userTaskAccessRepoMock
            .Setup(r => r.GetSharedTasksByUserIdAsync(userId, query.Page, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<UserTaskAccessEntity>(), 0));

        // Act
        Result<PagedResultDto<TaskDto>> result = await this._handler.Handle(query, CancellationToken.None);

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
        Guid userId = Guid.NewGuid();
        GetSharedTasksByUserIdQuery query = new GetSharedTasksByUserIdQuery(userId, 1, 10);

        TaskEntity task1 = new TaskEntity(userId, Guid.NewGuid(), TaskTitle.Create("Task"));
        List<UserTaskAccessEntity> entities = new List<UserTaskAccessEntity>
        {
            new(task1.Id, userId) { Task = task1, User = UserEntityFactory.Create() },
        };

        this._userTaskAccessRepoMock
            .Setup(r => r.GetSharedTasksByUserIdAsync(userId, query.Page, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((entities, 1));

        // Act
        Result<PagedResultDto<TaskDto>> result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.Equal(query.Page, result.Value.Page);
        Assert.Equal(task1.Id, result.Value.Items.First().Id);
    }
}
