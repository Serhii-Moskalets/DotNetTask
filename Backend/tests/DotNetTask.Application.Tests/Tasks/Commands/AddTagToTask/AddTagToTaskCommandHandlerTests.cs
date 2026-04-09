using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Tasks.Commands.AddTagToTask;
using DotNetTask.Domain.Common;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tests.Tasks.Commands.AddTagToTask;

/// <summary>
/// Unit tests for <see cref="AddTagToTaskCommandHandler"/>.
/// Ensures correct behavior when adding a tag to a task, including validation of task existence,
/// tag ownership, and idempotency checks.
/// </summary>
public class AddTagToTaskCommandHandlerTests
{
    private static readonly TaskTitle Title = TaskTitle.Create("Title");

    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITaskRepository> _taskRepoMock;
    private readonly Mock<ITagRepository> _tagRepoMock;
    private readonly AddTagToTaskCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddTagToTaskCommandHandlerTests"/> class.
    /// </summary>
    public AddTagToTaskCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._taskRepoMock = new Mock<ITaskRepository>();
        this._tagRepoMock = new Mock<ITagRepository>();

        this._uowMock.Setup(u => u.Tasks).Returns(this._taskRepoMock.Object);
        this._uowMock.Setup(u => u.Tags).Returns(this._tagRepoMock.Object);

        this._handler = new AddTagToTaskCommandHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Verifies that the handler returns a <see cref="ErrorCode.NotFound"/> result
    /// when the requested task does not exist or does not belong to the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTaskNotFound()
    {
        // Arrange
        this._taskRepoMock
            .Setup(r => r.GetTaskByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskEntity?)null);

        AddTagToTaskCommand command = new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Act
        Result<Unit> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
    }

    /// <summary>
    /// Verifies that the handler returns a success result without querying the tag repository
    /// if the task already has the requested tag assigned.
    /// </summary>
    /// <remarks>
    /// This test ensures the handler is idempotent and avoids unnecessary database round-trips.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenTagAlreadySet()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TaskEntity task = new(userId, Guid.NewGuid(), Title);
        Guid tagId = Guid.NewGuid();
        task.SetTag(tagId);

        this._taskRepoMock
            .Setup(r => r.GetTaskByIdForUserAsync(task.Id, userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        AddTagToTaskCommand command = new(task.Id, userId, tagId);

        // Act
        Result<Unit> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        this._tagRepoMock.Verify(t => t.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the handler correctly updates the task with the new tag ID
    /// and persists changes via the unit of work when all inputs are valid.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldSetTagSuccessfully_WhenAllValid()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TaskEntity task = new(userId, Guid.NewGuid(), Title);
        TagEntity tag = new(TagName.Create("Tag"), userId);

        this._taskRepoMock
            .Setup(r => r.GetTaskByIdForUserAsync(task.Id, userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        this._tagRepoMock
            .Setup(t => t.GetByIdAsync(tag.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        this._uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        AddTagToTaskCommand command = new(task.Id, userId, tag.Id);

        // Act
        Result<Unit> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.TagId.Should().Be(tag.Id);

        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the handler returns Not Found if the tag does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTagNotFound()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TaskEntity task = new(userId, Guid.NewGuid(), Title);
        Guid tagId = Guid.NewGuid();

        this._taskRepoMock
            .Setup(r => r.GetTaskByIdForUserAsync(task.Id, userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        this._tagRepoMock
            .Setup(r => r.GetByIdAsync(tagId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TagEntity?)null);

        AddTagToTaskCommand command = new(task.Id, userId, tagId);

        // Act
        Result<Unit> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Be(TagPolicy.NotFoundMessage);
    }

    /// <summary>
    /// Verifies that the handler returns InvalidOperation if the tag belongs to another user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTagOwnedByAnotherUser()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();

        TaskEntity task = new(userId, Guid.NewGuid(), Title);
        TagEntity tag = new(TagName.Create("Foreign Tag"), otherUserId);

        this._taskRepoMock
            .Setup(r => r.GetTaskByIdForUserAsync(task.Id, userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        this._tagRepoMock
            .Setup(r => r.GetByIdAsync(tag.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        AddTagToTaskCommand command = new(task.Id, userId, tag.Id);

        // Act
        Result<Unit> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.InvalidOperation);
        result.Error.Message.Should().Be(TagPolicy.DoNotHavePermission);
    }
}
