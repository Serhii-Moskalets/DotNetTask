using FluentAssertions;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Comment.Commands.DeleteComment;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Comment.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteCommentCommandHandler"/>.
/// Verifies validation, existence, and permission checks when deleting comments.
/// </summary>
public class DeleteCommentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ICommentRepository> _commentsRepoMock;
    private readonly Mock<ITaskRepository> _taskRepoMock;
    private readonly DeleteCommentCommandHandler _handler;

    private readonly CommentContent _content = CommentContent.Create("Test");

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteCommentCommandHandlerTests"/> class.
    /// </summary>
    public DeleteCommentCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._commentsRepoMock = new Mock<ICommentRepository>();
        this._taskRepoMock = new Mock<ITaskRepository>();

        this._uowMock.Setup(u => u.Comments).Returns(this._commentsRepoMock.Object);
        this._uowMock.Setup(u => u.Tasks).Returns(this._taskRepoMock.Object);

        this._handler = new DeleteCommentCommandHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Returns failure if the comment does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenCommentNotFound()
    {
        // Arrange
        this._commentsRepoMock
            .Setup(u => u.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CommentEntity?)null);

        var command = new DeleteCommentCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Be(CommentPolicy.CommentNotFoundMessage);
    }

    /// <summary>
    /// Returns failure if the user is not the owner of the task and cannot delete the comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenUserHasNoPermission()
    {
        // Arrange
        var comment = new CommentEntity(Guid.NewGuid(), Guid.NewGuid(), this._content);

        this._uowMock.Setup(u => u.Comments.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
               .ReturnsAsync(comment);
        this._uowMock.Setup(u => u.Tasks.IsTaskOwnerAsync(comment.TaskId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(false);

        var command = new DeleteCommentCommand(comment.Id, Guid.NewGuid());

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.InvalidOperation);
        result.Error.Message.Should().Be(CommentPolicy.DeleteAccessDeniedMessage);
    }

    /// <summary>
    /// Deletes the comment successfully when the user is the comment owner.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_ShouldDeleteComment_WhenUserIsCommentOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var comment = new CommentEntity(Guid.NewGuid(), userId, this._content);

        this._commentsRepoMock
            .Setup(u => u.GetByIdAsync(comment.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        var command = new DeleteCommentCommand(comment.Id, userId);

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this._commentsRepoMock.Verify(r => r.DeleteAsync(comment, It.IsAny<CancellationToken>()), Times.Once);
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Deletes the comment successfully when the user is the task owner.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_ShouldDeleteComment_WhenUserIsTaskOwner()
    {
        // Arrange
        var commentOwnerId = Guid.NewGuid();
        var taskOwnerId = Guid.NewGuid();
        var comment = new CommentEntity(Guid.NewGuid(), commentOwnerId, this._content);

        this._commentsRepoMock
             .Setup(u => u.GetByIdAsync(comment.Id, true, It.IsAny<CancellationToken>()))
             .ReturnsAsync(comment);

        this._taskRepoMock
            .Setup(t => t.IsTaskOwnerAsync(comment.TaskId, taskOwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new DeleteCommentCommand(comment.Id, taskOwnerId);

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this._commentsRepoMock.Verify(r => r.DeleteAsync(comment, It.IsAny<CancellationToken>()), Times.Once);
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
