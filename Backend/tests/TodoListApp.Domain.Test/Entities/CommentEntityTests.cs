using FluentAssertions;
using TinyResult.Enums;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Exceptions;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Test.Entities;

/// <summary>
/// Unit tests for the <see cref="CommentEntity"/> domain entity.
/// </summary>
public class CommentEntityTests
{
    private const string ValidText = "Hello";

    private static readonly FirstName FirstName = FirstName.Create("Test");
    private static readonly UserName UserName = UserName.Create("Test");
    private static readonly Email Email = Email.Create("test@test.com");
    private static readonly PasswordHash PasswordHash = PasswordHash.Create(new('a', 64));

    private readonly CommentContent _validContent = CommentContent.Create(ValidText);

    /// <summary>
    /// Verifies that the constructor successfully creates a <see cref="CommentEntity"/>
    /// when valid task ID, user ID, and content are provided.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateComment_WhenValidData()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var comment = new CommentEntity(taskId, userId, this._validContent);

        // Assert
        comment.TaskId.Should().Be(taskId);
        comment.UserId.Should().Be(userId);
        comment.Content.Value.Should().Be(ValidText);
        comment.CreatedDate.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    /// <summary>
    /// Verifies that the constructor throws a <see cref="DomainException"/>
    /// when the provided user entity ID does not match the specified user ID.
    /// </summary>
    [Fact]
    public void Constructor_ShouldThrow_WhenUserIdMismatch()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var differentUser = new UserEntity(FirstName, UserName, Email, PasswordHash);

        // Act
        var act = () => new CommentEntity(Guid.NewGuid(), userId, this._validContent, differentUser);

        // Assert
        act.Should().Throw<DomainException>();
    }

    /// <summary>
    /// Verifies that the <see cref="CommentEntity.Update"/> method correctly updates
    /// the comment's content and returns success when a different <see cref="CommentContent"/> is provided.
    /// </summary>
    [Fact]
    public void Update_ShouldChangeContent_WhenNewContentIsDifferent()
    {
        // Arrange
        var comment = new CommentEntity(Guid.NewGuid(), Guid.NewGuid(), this._validContent);
        var newContent = CommentContent.Create("Updated text");

        // Act
        var result = comment.Update(newContent);

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.Content.Should().Be(newContent);
    }

    /// <summary>
    /// Verifies that the <see cref="CommentEntity.Update"/> method returns a failure result
    /// with the correct error code and message when the new content is identical to the current one.
    /// </summary>
    [Fact]
    public void Update_ShouldReturnFailure_WhenContentIsSame()
    {
        // Arrange
        var comment = new CommentEntity(Guid.NewGuid(), Guid.NewGuid(), this._validContent);

        // Act
        var result = comment.Update(this._validContent);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(ErrorCode.InvalidOperation);
        result.Error.Message.Should().Be(CommentPolicy.NoChangesDetectedMessage);
        comment.Content.Should().Be(this._validContent);
    }
}
