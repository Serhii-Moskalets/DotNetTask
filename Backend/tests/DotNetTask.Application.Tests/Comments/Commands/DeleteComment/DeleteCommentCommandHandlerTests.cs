using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Comment.Commands.DeleteComment;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tests.Comment.Commands.DeleteComment;

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

        DeleteCommentCommand command = new(Guid.NewGuid(), Guid.NewGuid());

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

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
        CommentEntity comment = new(Guid.NewGuid(), Guid.NewGuid(), this._content);

        this._uowMock.Setup(u => u.Comments.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
               .ReturnsAsync(comment);
        this._uowMock.Setup(u => u.Tasks.IsTaskOwnerAsync(comment.TaskId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(false);

        DeleteCommentCommand command = new(comment.Id, Guid.NewGuid());

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

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
        Guid userId = Guid.NewGuid();
        CommentEntity comment = new(Guid.NewGuid(), userId, this._content);

        this._commentsRepoMock
            .Setup(u => u.GetByIdAsync(comment.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        DeleteCommentCommand command = new(comment.Id, userId);

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

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
        Guid commentOwnerId = Guid.NewGuid();
        Guid taskOwnerId = Guid.NewGuid();
        CommentEntity comment = new(Guid.NewGuid(), commentOwnerId, this._content);

        this._commentsRepoMock
             .Setup(u => u.GetByIdAsync(comment.Id, true, It.IsAny<CancellationToken>()))
             .ReturnsAsync(comment);

        this._taskRepoMock
            .Setup(t => t.IsTaskOwnerAsync(comment.TaskId, taskOwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        DeleteCommentCommand command = new(comment.Id, taskOwnerId);

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this._commentsRepoMock.Verify(r => r.DeleteAsync(comment, It.IsAny<CancellationToken>()), Times.Once);
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
