using FluentAssertions;
using Moq;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tag.Queries.GetTags;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Tag.Queries;

/// <summary>
/// Unit tests for <see cref="GetTagsQueryHandler"/>.
/// Verifies validation and retrieval of tags for a user.
/// </summary>
public class GetTagsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITagRepository> _tagRepoMock;
    private readonly GetTagsQueryHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTagsQueryHandlerTests"/> class.
    /// </summary>
    public GetTagsQueryHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._tagRepoMock = new Mock<ITagRepository>();

        this._uowMock.Setup(u => u.Tags).Returns(this._tagRepoMock.Object);

        this._handler = new GetTagsQueryHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Returns a list of tags when validation passes and tags exist for the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnTags_WhenTagsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var page = 1;
        var pageSize = 10;
        var tagEntities = new List<TagEntity>
        {
            new(TagName.Create("Tag1"), userId),
            new(TagName.Create("Tag2"), userId),
        };

        this._tagRepoMock
            .Setup(r => r.GetTagsAsync(userId, page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((tagEntities, tagEntities.Count));

        var query = new GetTagsQuery(userId, page, pageSize);

        // Act
        var result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        result.Value.Items.Select(x => x.Name)
            .Should()
            .ContainInOrder("Tag1", "Tag2");
    }

    /// <summary>
    /// Returns an empty list when the user has no tags.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoTagsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var page = 1;
        var pageSize = 10;

        this._tagRepoMock
            .Setup(r => r.GetTagsAsync(userId, page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<TagEntity>(), 0));

        var query = new GetTagsQuery(userId, page, pageSize);

        // Act
        var result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }
}
