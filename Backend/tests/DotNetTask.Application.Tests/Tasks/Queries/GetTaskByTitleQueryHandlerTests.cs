using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Common.Dtos;
using DotNetTask.Application.Tasks.Queries.GetTaskByTitle;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using TinyResult;

namespace DotNetTask.Application.Tests.Tasks.Queries;

/// <summary>
/// Unit tests for <see cref="GetTaskByTitleQueryHandler"/>.
/// Ensures that the handler correctly retrieves tasks by title for a specific user
/// and maps them to <see cref="TaskBriefDto"/> objects.
/// </summary>
public class GetTaskByTitleQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITaskRepository> _taskRepoMock;
    private readonly GetTaskByTitleQueryHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskByTitleQueryHandlerTests"/> class.
    /// </summary>
    public GetTaskByTitleQueryHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._taskRepoMock = new Mock<ITaskRepository>();

        this._uowMock.Setup(u => u.Tasks).Returns(this._taskRepoMock.Object);
        this._handler = new GetTaskByTitleQueryHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Verifies that the handler correctly calls the repository with provided filters
    /// and returns a paged result containing mapped <see cref="TaskBriefDto"/> objects.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnTaskDtos_WhenValidationPasses()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string text = "Task";
        int page = 1;
        int pageSize = 10;

        List<TaskEntity> taskEntities = new List<TaskEntity>
        {
            new(userId, Guid.NewGuid(), TaskTitle.Create("Task 1"), DateTime.UtcNow.AddDays(1)),
            new(userId, Guid.NewGuid(), TaskTitle.Create("Task 2"), DateTime.UtcNow.AddDays(2)),
        };

        this._taskRepoMock
            .Setup(r => r.SearchByTitleAsync(userId, text, page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((taskEntities, taskEntities.Count));

        GetTaskByTitleQuery query = new GetTaskByTitleQuery(userId, text, page, pageSize);

        // Act
        Result<PagedResultDto<TaskBriefDto>> result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalCount.Should().Be(taskEntities.Count);
        result.Value.Items.Should().HaveCount(taskEntities.Count);
        result.Value.Page.Should().Be(page);
        result.Value.Items.First().Title.Should().Be("Task 1");
    }

    /// <summary>
    /// Verifies that the handler returns an empty paged result without querying the database
    /// when the search text is null, empty, or consists only of whitespace.
    /// </summary>
    /// <param name="searchText">The search text to be tested, provided by <see cref="InlineDataAttribute"/>.</param>
    /// <remarks>
    /// This test ensures that the application performs early validation on the search input
    /// to prevent unnecessary repository calls and database load for invalid search queries.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_ShouldReturnEmptyPagedResult_WhenTextIsNullOrEmptyOrWhitespace(string? searchText)
    {
        // Arrange
        GetTaskByTitleQuery query = new GetTaskByTitleQuery(Guid.NewGuid(), searchText, 1, 10);

        // Act
        Result<PagedResultDto<TaskBriefDto>> result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);

        this._taskRepoMock.Verify(
            r =>
            r.SearchByTitleAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
