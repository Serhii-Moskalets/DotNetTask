using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Comment.Queries.GetComments;
using DotNetTask.Application.Common.Dtos;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Test.Common;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tests.Comment.Queries;

/// <summary>
/// Unit tests for <see cref="GetCommentsQueryHandler"/>.
/// Verifies access checks and retrieval of comments for a given task.
/// </summary>
public class GetCommentsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITaskAccessService> _taskAccessMock;
    private readonly Mock<ICommentRepository> _commentsRepoMock;
    private readonly GetCommentsQueryHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCommentsQueryHandlerTests"/> class.
    /// </summary>
    public GetCommentsQueryHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._taskAccessMock = new Mock<ITaskAccessService>();
        this._commentsRepoMock = new Mock<ICommentRepository>();

        this._uowMock.Setup(u => u.Comments).Returns(this._commentsRepoMock.Object);

        this._handler = new GetCommentsQueryHandler(this._uowMock.Object, this._taskAccessMock.Object);
    }

    /// <summary>
    /// Returns failure if the user does not have access to the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserHasNoAccess()
    {
        // Arrange
        Guid taskId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        this._taskAccessMock
            .Setup(s => s.HasAccessAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        GetCommentsQuery query = new GetCommentsQuery(taskId, userId);

        // Act
        Result<PagedResultDto<CommentDto>> result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.InvalidOperation);
        result.Error.Message.Should().Be(TaskPolicy.AccessDeniedMessage);
    }

    /// <summary>
    /// Returns the list of comments successfully when the user has access to the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnComments_WhenUserHasAccess()
    {
        // Arrange
        Guid taskId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        int page = 1;
        int pageSize = 10;

        UserEntity author = UserEntityFactory.Create();
        List<CommentEntity> comments = new List<CommentEntity>
        {
            new(taskId, author.Id, CommentContent.Create("Comment_1"), author),
            new(taskId, author.Id, CommentContent.Create("Comment_2"), author),
        };

        this._taskAccessMock
            .Setup(s => s.HasAccessAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        this._commentsRepoMock
            .Setup(r => r.GetCommentsByTaskIdAsync(taskId, page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((comments, comments.Count));

        GetCommentsQuery query = new GetCommentsQuery(taskId, userId, page, pageSize);

        // Act
        Result<PagedResultDto<CommentDto>> result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalCount.Should().Be(2);
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.First().Content.Should().Be("Comment_1");
    }
}
