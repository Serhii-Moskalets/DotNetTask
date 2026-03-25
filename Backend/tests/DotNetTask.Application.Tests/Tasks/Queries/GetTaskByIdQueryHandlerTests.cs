using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Common.Dtos;
using DotNetTask.Application.Tasks.Queries.GetTaskById;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tests.Tasks.Queries;

/// <summary>
/// Unit tests for <see cref="GetTaskByIdQueryHandler"/>.
/// Verifies the behavior of the handler for retrieving a task by its ID.
/// </summary>
public class GetTaskByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITaskRepository> _taskRepoMock;
    private readonly GetTaskByIdQueryHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskByIdQueryHandlerTests"/> class.
    /// </summary>
    public GetTaskByIdQueryHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._taskRepoMock = new Mock<ITaskRepository>();

        this._uowMock.Setup(u => u.Tasks).Returns(this._taskRepoMock.Object);
        this._handler = new GetTaskByIdQueryHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Ensures the handler returns NotFound when the task does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        this._taskRepoMock
            .Setup(r => r.GetTaskByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskEntity?)null);

        GetTaskByIdQuery query = new GetTaskByIdQuery(Guid.NewGuid(), Guid.NewGuid());

        // Act
        Result<TaskDto> result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Be(TaskPolicy.NotFoundMessage);
    }

    /// <summary>
    /// Ensures the handler returns a valid task DTO when the task exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnTaskDto_WhenTaskExists()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TaskEntity task = new TaskEntity(userId, Guid.NewGuid(), TaskTitle.Create("Task"), DateTime.UtcNow.AddDays(1));

        this._taskRepoMock
            .Setup(r => r.GetTaskByIdForUserAsync(
                task.Id,
                userId,
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        GetTaskByIdQuery query = new GetTaskByIdQuery(userId, task.Id);

        // Act
        Result<TaskDto> result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be(task.Title.Value);
        result.Value.Id.Should().Be(task.Id);

        this._taskRepoMock.Verify(
            r => r.GetTaskByIdForUserAsync(
                task.Id, userId, It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
