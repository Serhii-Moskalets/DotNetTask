using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Exceptions;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Domain.Test.Entities;

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
        Guid taskId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        // Act
        CommentEntity comment = new(taskId, userId, this._validContent);

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
        Guid userId = Guid.NewGuid();
        UserEntity differentUser = new(FirstName, UserName, Email, PasswordHash);

        // Act
        Func<CommentEntity> act = () => new CommentEntity(Guid.NewGuid(), userId, this._validContent, differentUser);

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
        CommentEntity comment = new(Guid.NewGuid(), Guid.NewGuid(), this._validContent);
        CommentContent newContent = CommentContent.Create("Updated text");

        // Act
        Result<bool> result = comment.Update(newContent);

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
        CommentEntity comment = new(Guid.NewGuid(), Guid.NewGuid(), this._validContent);

        // Act
        Result<bool> result = comment.Update(this._validContent);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(ErrorCode.InvalidOperation);
        result.Error.Message.Should().Be(CommentPolicy.NoChangesDetectedMessage);
        comment.Content.Should().Be(this._validContent);
    }
}
