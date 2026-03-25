using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Tag.Commands.CreateTag;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tests.Tag.Commands;

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
    private readonly Mock<IUniqueValueService> _uniqueNameServiceMock;
    private readonly CreateTagCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTagCommandHandlerTests"/> class.
    /// </summary>
    public CreateTagCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._tagRepoMock = new Mock<ITagRepository>();
        this._taskRepoMock = new Mock<ITaskRepository>();
        this._uniqueNameServiceMock = new Mock<IUniqueValueService>();

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
        Guid userId = Guid.NewGuid();
        TaskEntity task = new TaskEntity(userId, Guid.NewGuid(), TaskTitle.Create("Title"));
        CreateTagCommand command = new CreateTagCommand(userId, task.Id, "Tag");
        TagName expectedTagName = TagName.Create("Tag");

        this._uniqueNameServiceMock
            .Setup(s => s.GetUniqueValueAsync<TagName>(
                It.IsAny<string>(),
                It.IsAny<Func<string, TagName>>(),
                It.IsAny<Func<TagName, CancellationToken, Task<bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTagName);

        this._taskRepoMock
            .Setup(r => r.GetTaskByIdForUserAsync(task.Id, userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        // Act
        Result<Guid> result = await this._handler.Handle(command, CancellationToken.None);

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
        Guid userId = Guid.NewGuid();
        CreateTagCommand command = new CreateTagCommand(userId, Guid.NewGuid(), "Tag");

        this._taskRepoMock
            .Setup(r => r.GetTaskByIdForUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskEntity?)null);

        // Act
        Result<Guid> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);

        this._uniqueNameServiceMock.Verify(
        s => s.GetUniqueValueAsync<TagName>(
            It.IsAny<string>(),
            It.IsAny<Func<string, TagName>>(),
            It.IsAny<Func<TagName, CancellationToken, Task<bool>>>(),
            It.IsAny<CancellationToken>()),
        Times.Never);

        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
