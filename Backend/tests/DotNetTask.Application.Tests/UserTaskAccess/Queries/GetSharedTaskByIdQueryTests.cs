using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Common.Dtos;
using DotNetTask.Application.UserTaskAccess.Mappers;
using DotNetTask.Application.UserTaskAccess.Queries.GetSharedTaskById;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Test.Common;
using DotNetTask.Domain.ValueObjects;
using FluentAssertions;
using Moq;

using TinyResult;

namespace DotNetTask.Application.Tests.UserTaskAccess.Queries;

/// <summary>
/// Unit tests for <see cref="GetSharedTaskByIdQueryHandler"/>.
/// Verifies behavior when retrieving a shared task by its ID and user ID.
/// </summary>
public class GetSharedTaskByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserTaskAccessRepository> _userTaskAccessRepoMock;
    private readonly GetSharedTaskByIdQueryHandler _handler;

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
        Guid taskId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        GetSharedTaskByIdQuery query = new GetSharedTaskByIdQuery(taskId, userId);

        this._userTaskAccessRepoMock
            .Setup(r => r.GetByTaskAndUserIdAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserTaskAccessEntity?)null);

        // Act
        Result<TaskDto> result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(TinyResult.Enums.ErrorCode.NotFound);
        result.Error.Message.Should().Be(TaskPolicy.NotFoundMessage);
    }

    /// <summary>
    /// Ensures that the handler returns the mapped task DTO when the task exists and is shared with the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnTaskDto_WhenTaskExists()
    {
        // Arrange
        Guid taskId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        UserTaskAccessEntity taskAccess = new UserTaskAccessEntity(taskId, userId)
        {
            Task = new TaskEntity(userId, Guid.NewGuid(), TaskTitle.Create("Task")),
            User = UserEntityFactory.Create(),
        };

        this._userTaskAccessRepoMock
            .Setup(r => r.GetByTaskAndUserIdAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskAccess);

        GetSharedTaskByIdQuery query = new GetSharedTaskByIdQuery(taskId, userId);

        // Act
        Result<TaskDto> result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        TaskDto mapped = TaskAccessForUserMapper.Map(taskAccess);
        result.Value!.Id.Should().Be(mapped.Id);
    }
}
