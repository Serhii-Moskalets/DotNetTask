using FluentValidation.TestHelper;
using TodoListApp.Application.Comment.Commands.UpdateComment;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Comment.Commands;

/// <summary>
/// Unit tests for <see cref="UpdateCommentCommandValidator"/>.
/// Ensures that the validator correctly enforces rules for updating a comment.
/// </summary>
public class UpdateCommentCommandValidatorTests
{
    private readonly UpdateCommentCommandValidator _validator = new();

    /// <summary>
    /// Fails validation when UserId is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var command = new UpdateCommentCommand(Guid.NewGuid(), Guid.Empty, "Some text");

        var result = this._validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.UserId)
              .WithErrorMessage(UserPolicy.UserIdRequiredMessage);
    }

    /// <summary>
    /// Fails validation when CommentId is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_CommentId_Is_Empty()
    {
        var command = new UpdateCommentCommand(Guid.Empty, Guid.NewGuid(), "Some text");

        var result = this._validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.CommentId)
              .WithErrorMessage(CommentPolicy.CommentIdRequiredMessage);
    }

    /// <summary>
    /// Fails validation when NewText is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_NewText_Is_Empty()
    {
        var command = new UpdateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), string.Empty);

        var result = this._validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.NewContent)
              .WithErrorMessage(CommentPolicy.EmptyMessage);
    }

    /// <summary>
    /// Fails validation when NewText exceeds maximum allowed length.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_NewText_Exceeds_MaxLength()
    {
        var longText = new string('a', CommentContent.MaxLength + 1);
        var command = new UpdateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), longText);

        var result = this._validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.NewContent)
              .WithErrorMessage(CommentPolicy.TooLongMessage);
    }

    /// <summary>
    /// Passes validation when the command is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new UpdateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), "Valid comment");

        var result = this._validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
