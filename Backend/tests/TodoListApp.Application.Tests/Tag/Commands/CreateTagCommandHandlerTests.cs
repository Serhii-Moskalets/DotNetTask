using FluentAssertions;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.Services;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tag.Commands.CreateTag;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Tag.Commands;

/// <summary>
/// Unit tests for <see cref="CreateTagCommandHandler"/>.
/// Verifies validation handling, tag creation,
/// and automatic name suffixing when duplicates exist.
/// </summary>
public class CreateTagCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITagRepository> _tagRepoMock;
    private readonly Mock<ITaskRepository> _taskRepoMock;
    private readonly Mock<IUniqueNameService> _uniqueNameServiceMock;
    private readonly CreateTagCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTagCommandHandlerTests"/> class.
    /// </summary>
    public CreateTagCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._tagRepoMock = new Mock<ITagRepository>();
        this._taskRepoMock = new Mock<ITaskRepository>();
        this._uniqueNameServiceMock = new Mock<IUniqueNameService>();

        this._uowMock.Setup(u => u.Tags).Returns(this._tagRepoMock.Object);
        this._uowMock.Setup(u => u.Tasks).Returns(this._taskRepoMock.Object);

        this._handler = new CreateTagCommandHandler(
            this._uowMock.Object,
            this._uniqueNameServiceMock.Object);
    }

    /// <summary>
    /// Ensures that a new tag is created successfully when the provided name is unique for the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldAddTag_WhenNameIsUnique()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var task = new TaskEntity(userId, Guid.NewGuid(), TaskTitle.Create("Title"));

        var command = new CreateTagCommand(userId, task.Id, "Tag");

        this._uniqueNameServiceMock
            .Setup(s => s.GetUniqueNameAsync(
                It.IsAny<string>(),
                It.IsAny<Func<string,
                CancellationToken,
                Task<bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("Tag");

        this._taskRepoMock
            .Setup(r => r.GetTaskByIdForUserAsync(task.Id, userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.Should().Be(task.TagId!.Value);
        this._tagRepoMock.Verify(
            r => r.AddAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()),
            Times.Once);

        this._tagRepoMock.Verify(
            r => r.AddAsync(It.Is<TagEntity>(t => t.Name.Value == "Tag"), It.IsAny<CancellationToken>()),
            Times.Once);
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Ensures that the handler returns a failure result with <see cref="ErrorCode.NotFound"/>
    /// and does not persist any changes if the specified task is not found.
    /// </summary>
    /// <remarks>
    /// This test verifies that the system maintains integrity by preventing the creation
    /// of orphaned tags that aren't linked to a valid task.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateTagCommand(userId, Guid.NewGuid(), "Tag");

        this._uniqueNameServiceMock
            .Setup(s => s.GetUniqueNameAsync(
                It.IsAny<string>(),
                It.IsAny<Func<string,
                CancellationToken,
                Task<bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("Tag");

        this._taskRepoMock
            .Setup(r => r.GetTaskByIdForUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskEntity?)null);

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
