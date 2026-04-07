using DotNetTask.Application.Comment.Commands.UpdateComment;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.ValueObjects;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Comment.Commands;

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
        UpdateCommentCommand command = new(Guid.NewGuid(), Guid.Empty, "Some text");

        TestValidationResult<UpdateCommentCommand> result = this._validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Fails validation when CommentId is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_CommentId_Is_Empty()
    {
        UpdateCommentCommand command = new(Guid.Empty, Guid.NewGuid(), "Some text");

        TestValidationResult<UpdateCommentCommand> result = this._validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.CommentId)
              .WithErrorMessage(CommentPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Fails validation when NewText is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_NewText_Is_Empty()
    {
        UpdateCommentCommand command = new(Guid.NewGuid(), Guid.NewGuid(), string.Empty);

        TestValidationResult<UpdateCommentCommand> result = this._validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.NewContent)
              .WithErrorMessage(CommentPolicy.EmptyMessage);
    }

    /// <summary>
    /// Fails validation when NewText exceeds maximum allowed length.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_NewText_Exceeds_MaxLength()
    {
        string longText = new('a', CommentContent.MaxLength + 1);
        UpdateCommentCommand command = new(Guid.NewGuid(), Guid.NewGuid(), longText);

        TestValidationResult<UpdateCommentCommand> result = this._validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.NewContent)
              .WithErrorMessage(CommentPolicy.TooLongMessage);
    }

    /// <summary>
    /// Passes validation when the command is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        UpdateCommentCommand command = new(Guid.NewGuid(), Guid.NewGuid(), "Valid comment");

        TestValidationResult<UpdateCommentCommand> result = this._validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
