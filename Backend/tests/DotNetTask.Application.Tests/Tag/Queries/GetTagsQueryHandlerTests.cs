using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Common.Dtos;
using DotNetTask.Application.Tag.Queries.GetTags;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using TinyResult;

namespace DotNetTask.Application.Tests.Tag.Queries;

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
        Guid userId = Guid.NewGuid();
        int page = 1;
        int pageSize = 10;
        List<TagEntity> tagEntities = new()
        {
            new(TagName.Create("Tag1"), userId),
            new(TagName.Create("Tag2"), userId),
        };

        this._tagRepoMock
            .Setup(r => r.GetTagsAsync(userId, page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((tagEntities, tagEntities.Count));

        GetTagsQuery query = new(userId, page, pageSize);

        // Act
        Result<PagedResultDto<TagDto>> result = await this._handler.Handle(query, CancellationToken.None);

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
        Guid userId = Guid.NewGuid();
        int page = 1;
        int pageSize = 10;

        this._tagRepoMock
            .Setup(r => r.GetTagsAsync(userId, page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<TagEntity>(), 0));

        GetTagsQuery query = new(userId, page, pageSize);

        // Act
        Result<PagedResultDto<TagDto>> result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }
}
