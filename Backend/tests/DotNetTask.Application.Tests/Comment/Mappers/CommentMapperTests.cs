using DotNetTask.Application.Comment.Mappers;
using DotNetTask.Application.Common.Dtos;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Test.Common;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

namespace DotNetTask.Application.Tests.Comment.Mappers;

/// <summary>
/// Unit tests for <see cref="CommentMapper"/>.
/// </summary>
public class CommentMapperTests
{
    private readonly CommentContent _content = CommentContent.Create("Content");

    /// <summary>
    /// Verifies that all properties are correctly mapped from <see cref="CommentEntity"/> to <see cref="CommentDto"/>.
    /// </summary>
    [Fact]
    public void Map_CommentEntityToCommentDto_ReturnsCorrectMappedDto()
    {
        // Arrange
        Guid taskId = Guid.NewGuid();
        UserEntity user = UserEntityFactory.Create();
        CommentEntity comment = new(taskId, user.Id, this._content, user);

        // Act
        CommentDto result = CommentMapper.Map(comment);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be(comment.Content.Value);
        result.CreatedDate.Should().Be(comment.CreatedDate);

        result.User.Should().NotBeNull();
        result.User.UserName.Should().Be(user.UserName.Value);
        result.User.FirstName.Should().Be(user.FirstName.Value);
        result.User.LastName.Should().Be(user.LastName!.Value);
        result.User.Email.Should().Be(user.Email.Value);
    }

    /// <summary>
    /// Verifies that mapping a collection of entities returns the same number of DTOs with correct data.
    /// </summary>
    [Fact]
    public void Map_CollectionOfEntities_ReturnsMappedDtoCollection()
    {
        // Arrange
        Guid taskId = Guid.NewGuid();
        UserEntity user = UserEntityFactory.Create();
        List<CommentEntity> entities = new()
        {
            new(taskId, user.Id, this._content, user),
            new(taskId, user.Id, CommentContent.Create("Content_2"), user),
        };

        // Act
        IReadOnlyCollection<CommentDto> result = CommentMapper.Map(entities);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(entities.Count);
        result.Select(r => r.Content).Should().ContainInOrder(
            entities[0].Content.Value,
            entities[1].Content.Value);
    }

    /// <summary>
    /// Verifies that mapping a <see cref="UserEntity"/> to <see cref="UserBriefDto"/> works independently.
    /// </summary>
    [Fact]
    public void Map_UserEntityToUserBriefDto_ReturnsCorrectDto()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();

        // Act
        UserBriefDto result = CommentMapper.Map(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.UserName.Should().Be(user.UserName.Value);
        result.FirstName.Should().Be(user.FirstName.Value);
        result.LastName.Should().Be(user.LastName!.Value);
        result.Email.Should().Be(user.Email.Value);
    }
}
