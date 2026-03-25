using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Common.Dtos;
using DotNetTask.Application.TaskList.Dtos;
using DotNetTask.Application.TaskList.Queries.GetTaskLists;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using Moq;

namespace DotNetTask.Application.Tests.TaskList.Queries;

/// <summary>
/// Unit tests for <see cref="GetTaskListsQueryHandler"/>.
/// Verifies that the handler correctly retrieves and maps task list entities for a specific user.
/// </summary>
public class GetAllTaskListQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITaskListRepository> _taskListRepoMock;
    private readonly GetTaskListsQueryHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllTaskListQueryHandlerTests"/> class.
    /// </summary>
    public GetAllTaskListQueryHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._taskListRepoMock = new Mock<ITaskListRepository>();

        this._uowMock.Setup(u => u.TaskLists).Returns(this._taskListRepoMock.Object);
        this._handler = new GetTaskListsQueryHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Verifies that <see cref="GetTaskListsQueryHandler.Handle"/> returns a successful result
    /// containing a collection of mapped DTOs when task lists exist for the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>[Fact]
    public async Task Handle_ShouldReturnTaskListDtos_WhenListsExist()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        int page = 1;
        int pageSize = 10;
        List<TaskListEntity> entities = new List<TaskListEntity>
        {
            new(userId, TaskListTitle.Create("List 1")),
            new(userId, TaskListTitle.Create("List 2")),
        };

        this._taskListRepoMock
            .Setup(r => r.GetTaskListsAsync(userId, page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((entities, entities.Count));

        GetTaskListsQuery query = new GetTaskListsQuery(userId);

        // Act
        TinyResult.Result<PagedResultDto<TaskListDto>> result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        PagedResultDto<TaskListDto> value = result.Value!;
        value.TotalCount.Should().Be(entities.Count);
        value.Items.Should().HaveCount(2);

        value.Items.Should().Contain(t => t.Title == "List 1");
        value.Items.Select(x => x.Title).Should().BeEquivalentTo(entities.Select(e => e.Title.Value));

        this._taskListRepoMock.Verify(r => r.GetTaskListsAsync(userId, page, pageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="GetTaskListsQueryHandler.Handle"/> returns a success result
    /// with an empty collection when the user has no task lists in the repository.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>[Fact]
    public async Task Handle_ShouldReturnEmptyCollection_WhenNoListsExist()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        int page = 1;
        int pageSize = 10;

        this._taskListRepoMock
            .Setup(r => r.GetTaskListsAsync(userId, page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<TaskListEntity>(), 0));

        GetTaskListsQuery query = new GetTaskListsQuery(userId, page, pageSize);

        // Act
        TinyResult.Result<PagedResultDto<TaskListDto>> result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }
}
