using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Comment.Commands.UpdateComment;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tests.Comment.Commands.UpdateComment;

/// <summary>
/// Unit tests for <see cref="UpdateCommentCommandHandler"/>.
/// Verifies validation, existence, permission checks, and comment updating.
/// </summary>
public class UpdateCommentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ICommentRepository> _commentsRepoMock;
    private readonly UpdateCommentCommandHandler _handler;

    private readonly CommentContent _oldContent = CommentContent.Create("Test");

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCommentCommandHandlerTests"/> class.
    /// </summary>
    public UpdateCommentCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._commentsRepoMock = new Mock<ICommentRepository>();

        this._uowMock.Setup(u => u.Comments).Returns(this._commentsRepoMock.Object);

        this._handler = new UpdateCommentCommandHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Returns failure if the comment does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenCommentNotFound()
    {
        // Arrange
        this._uowMock.Setup(u => u.Comments.GetByIdAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
               .ReturnsAsync((CommentEntity?)null);

        UpdateCommentCommand command = new(Guid.NewGuid(), Guid.NewGuid(), "New Text");

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Be(CommentPolicy.CommentNotFoundMessage);
    }

    /// <summary>
    /// Returns failure if the user is not the owner of the comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenUserIsNotOwner()
    {
        // Arrange
        CommentEntity comment = new(Guid.NewGuid(), Guid.NewGuid(), this._oldContent);

        this._uowMock.Setup(u => u.Comments.GetByIdAsync(comment.Id, false, It.IsAny<CancellationToken>()))
               .ReturnsAsync(comment);

        UpdateCommentCommand command = new(comment.Id, Guid.NewGuid(), "New Text");

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.InvalidOperation);
        result.Error.Message.Should().Be(CommentPolicy.UpdateAccessDeniedMessage);
    }

    /// <summary>
    /// Verifies that a failure is returned when trying to update with the same content.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenContentIsSame()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        CommentEntity comment = new(Guid.NewGuid(), userId, this._oldContent);

        this._commentsRepoMock.Setup(r => r.GetByIdAsync(comment.Id, false, It.IsAny<CancellationToken>()))
               .ReturnsAsync(comment);

        UpdateCommentCommand command = new(comment.Id, userId, this._oldContent.Value);

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(ErrorCode.InvalidOperation);
        result.Error.Message.Should().Be(CommentPolicy.NoChangesDetectedMessage);
    }

    /// <summary>
    /// Updates the comment successfully when the user is the owner.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_ShouldUpdateComment_WhenUserIsOwner()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        CommentEntity comment = new(Guid.NewGuid(), userId, this._oldContent);

        this._uowMock.Setup(u => u.Comments.GetByIdAsync(comment.Id, false, It.IsAny<CancellationToken>()))
               .ReturnsAsync(comment);
        this._uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        UpdateCommentCommandHandler handler = new(this._uowMock.Object);

        UpdateCommentCommand command = new(comment.Id, userId, "New Text");

        // Act
        Result<bool> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.Content.Value.Should().Be("New Text");
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
