using FluentAssertions;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tag.Commands.DeleteTag;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Tag.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTagCommandHandler"/>.
/// Verifies validation handling, tag existence checks, and deletion behavior.
/// </summary>
public class DeleteTagCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITagRepository> _tagRepoMock;
    private readonly DeleteTagCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTagCommandHandlerTests"/> class.
    /// </summary>
    public DeleteTagCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._tagRepoMock = new Mock<ITagRepository>();

        this._uowMock.Setup(u => u.Tags).Returns(this._tagRepoMock.Object);

        this._handler = new DeleteTagCommandHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Returns a failure result when the tag does not exist for the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTagNotFound()
    {
        // Arrange
        this._tagRepoMock.Setup(r => r.GetTagByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
                   .ReturnsAsync((TagEntity?)null);

        var command = new DeleteTagCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Be(TagPolicy.NotFoundMessage);
    }

    /// <summary>
    /// Deletes the tag successfully when validation passes and the tag exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteTag_WhenValidationPassesAndTagExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tagEntity = new TagEntity(TagName.Create("Tag"), userId);

        this._tagRepoMock.Setup(r => r.GetTagByIdForUserAsync(tagEntity.Id, userId, false, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(tagEntity);
        this._tagRepoMock.Setup(r => r.DeleteAsync(tagEntity, It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

        this._uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteTagCommand(tagEntity.Id, userId);

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this._tagRepoMock.Verify(r => r.DeleteAsync(tagEntity, It.IsAny<CancellationToken>()), Times.Once);
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
