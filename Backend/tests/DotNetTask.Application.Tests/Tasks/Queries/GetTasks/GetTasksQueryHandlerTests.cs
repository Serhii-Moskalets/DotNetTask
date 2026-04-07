using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Common.Dtos;
using DotNetTask.Application.Tasks.Queries.GetTasks;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Enums;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using TinyResult;

namespace DotNetTask.Application.Tests.Tasks.Queries.GetTasks;

/// <summary>
/// Unit tests for <see cref="GetTasksQueryHandler"/>.
/// Verifies validation handling, successful task retrieval,
/// and correct repository interaction with query parameters.
/// </summary>
public class GetTasksQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly GetTasksQueryHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTasksQueryHandlerTests"/> class.
    /// </summary>
    public GetTasksQueryHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._taskRepositoryMock = new Mock<ITaskRepository>();

        this._unitOfWorkMock.Setup(u => u.Tasks).Returns(this._taskRepositoryMock.Object);
        this._handler = new GetTasksQueryHandler(this._unitOfWorkMock.Object);
    }

    /// <summary>
    /// Verifies that <see cref="GetTasksQueryHandler.Handle"/> returns a successful paged result
    /// containing the expected total count and mapped items when valid parameters are provided.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenValidationPasses()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid taskListId = Guid.NewGuid();
        GetTasksQuery query = new(userId, taskListId)
        {
            Page = 1,
            PageSize = 10,
        };

        List<TaskEntity> entities =
        [
            new(userId, taskListId, TaskTitle.Create("Task 1")),
            new(userId, taskListId, TaskTitle.Create("Task 2")),
        ];

        this._taskRepositoryMock
            .Setup(r => r.GetTasksAsync(
                query.UserId,
                query.TaskListId,
                query.Page,
                query.PageSize,
                query.TaskStatuses,
                query.DueBefore,
                query.DueAfter,
                query.TaskSortBy,
                query.Ascending,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((entities, 2));

        // Act
        Result<PagedResultDto<TaskBriefDto>> result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalCount.Should().Be(2);
        result.Value.Page.Should().Be(query.Page);
        result.Value.Items.Should().HaveCount(2);
    }

    /// <summary>
    /// Verifies that all filtering, sorting, and pagination parameters from the <see cref="GetTasksQuery"/>
    /// are correctly mapped and passed to the <see cref="ITaskRepository.GetTasksAsync"/> method.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldCallRepository_WithExactQueryParameters()
    {
        // Arrange
        GetTasksQuery query = new(
            UserId: Guid.NewGuid(),
            TaskListId: Guid.NewGuid(),
            TaskStatuses: [StatusTask.NotStarted],
            DueBefore: DateTime.UtcNow.AddDays(1),
            DueAfter: DateTime.UtcNow,
            TaskSortBy: TaskSortBy.Title,
            Ascending: false)
        {
            Page = 2,
            PageSize = 5,
        };

        this._taskRepositoryMock
            .Setup(r => r.GetTasksAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<IReadOnlyCollection<StatusTask>?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<TaskSortBy>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<TaskEntity>(), 0));

        // Act
        await this._handler.Handle(query, CancellationToken.None);

        // Assert
        this._taskRepositoryMock.Verify(
            r => r.GetTasksAsync(
                query.UserId,
                query.TaskListId,
                query.Page,
                query.PageSize,
                query.TaskStatuses,
                query.DueBefore,
                query.DueAfter,
                query.TaskSortBy,
                query.Ascending,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
