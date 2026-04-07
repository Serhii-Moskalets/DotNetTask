using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Comment.Commands.CreateComment;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;

using FluentAssertions;

using Moq;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tests.Comment.Commands;

/// <summary>
/// Unit tests for <see cref="CreateCommentCommandHandler"/>.
/// Verifies validation, access checks, and comment creation.
/// </summary>
public class CreateCommentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITaskAccessService> _taskAccessMock;
    private readonly Mock<ICommentRepository> _commentsRepoMock;
    private readonly CreateCommentCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCommentCommandHandlerTests"/> class.
    /// </summary>
    public CreateCommentCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._taskAccessMock = new Mock<ITaskAccessService>();
        this._commentsRepoMock = new Mock<ICommentRepository>();

        this._uowMock.Setup(u => u.Comments).Returns(this._commentsRepoMock.Object);

        this._handler = new CreateCommentCommandHandler(this._uowMock.Object, this._taskAccessMock.Object);
    }

    /// <summary>
    /// Returns failure if user does not have access to the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenUserHasNoAccess()
    {
        // Arrange
        this._taskAccessMock
             .Setup(s => s.HasAccessAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);

        CreateCommentCommand command = new(Guid.NewGuid(), Guid.NewGuid(), "Text");

        // Act
        Result<Guid> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be(TaskPolicy.AccessDeniedMessage);
        result.Error.Code.Should().Be(ErrorCode.InvalidOperation);
    }

    /// <summary>
    /// Adds a comment successfully when validation passes and user has access.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_ShouldAddComment_WhenValidationPasses()
    {
        // Arrange
        Guid taskId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        string text = "Valid comment";
        CreateCommentCommand command = new(taskId, userId, text);

        this._taskAccessMock
            .Setup(s => s.HasAccessAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        this._uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        Result<Guid> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        this._commentsRepoMock.Verify(
            r => r.AddAsync(
                It.Is<CommentEntity>(c => c.TaskId == taskId && c.UserId == userId && c.Content.Value == text),
                It.IsAny<CancellationToken>()),
            Times.Once);

        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
