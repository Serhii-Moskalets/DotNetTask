using TodoListApp.Application.Comment.Mappers;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Comment.Mappers;

/// <summary>
/// Unit tests for <see cref="CommentMapper"/>.
/// </summary>
public class CommentMapperTests
{
    private readonly string _passwordHash = new('a', 64);

    /// <summary>
    /// Verifies that all properties are correctly mapped from <see cref="CommentEntity"/> to <see cref="CommentDto"/>.
    /// </summary>
    [Fact]
    public void Map_CommentEntityToCommentDto_ReturnsCorrectMappedDto()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var user = new UserEntity("John", "johnd", "john@example.com", this._passwordHash, "Joe");
        var comment = new CommentEntity(taskId, user.Id, "This is a test comment", user);

        // Act
        var result = CommentMapper.Map(comment);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(comment.Text, result.Text);
        Assert.Equal(comment.CreatedDate, result.CreatedDate);

        Assert.NotNull(result.User);
        Assert.Equal(user.UserName.Value, result.User.UserName);
        Assert.Equal(user.FirstName.Value, result.User.FirstName);
        Assert.Equal(user.LastName, result.User.LastName);
        Assert.Equal(user.Email.Value, result.User.Email);
    }

    /// <summary>
    /// Verifies that mapping a collection of entities returns the same number of DTOs with correct data.
    /// </summary>
    [Fact]
    public void Map_CollectionOfEntities_ReturnsMappedDtoCollection()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var user = new UserEntity("Alice", "alice", "alice@test.com", this._passwordHash);
        var entities = new List<CommentEntity>
        {
            new(taskId, user.Id, "Comment 1", user),
            new(taskId, user.Id, "Comment 2", user),
        };

        // Act
        var result = CommentMapper.Map(entities);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entities.Count, result.Count);
        Assert.Equal(entities[0].Text, result.First().Text);
        Assert.Equal(entities[1].Text, result.Last().Text);
    }

    /// <summary>
    /// Verifies that mapping a <see cref="UserEntity"/> to <see cref="UserBriefDto"/> works independently.
    /// </summary>
    [Fact]
    public void Map_UserEntityToUserBriefDto_ReturnsCorrectDto()
    {
        // Arrange
        var user = new UserEntity("John", "johnd", "john@example.com", this._passwordHash, "Joe");

        // Act
        var result = CommentMapper.Map(user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.UserName.Value, result.UserName);
        Assert.Equal(user.FirstName.Value, result.FirstName);
        Assert.Equal(user.Email.Value, result.Email);
        Assert.Equal(user.LastName, result.LastName);
    }
}
