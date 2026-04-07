using DotNetTask.Application.Comment.Commands.DeleteComment;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Comment.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteCommentCommandValidator"/>.
/// Validates command properties and ensures correct validation messages.
/// </summary>
public class DeleteCommentCommandValidatorTests
{
    private readonly DeleteCommentCommandValidator _validator = new();

    /// <summary>
    /// Fails validation when UserId is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        DeleteCommentCommand command = new(Guid.NewGuid(), Guid.Empty);

        TestValidationResult<DeleteCommentCommand> result = this._validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Fails validation when CommentId is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_CommentId_Is_Empty()
    {
        DeleteCommentCommand command = new(Guid.Empty, Guid.NewGuid());

        TestValidationResult<DeleteCommentCommand> result = this._validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.CommentId)
              .WithErrorMessage(TaskPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Passes validation when command is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        DeleteCommentCommand command = new(Guid.NewGuid(), Guid.NewGuid());

        TestValidationResult<DeleteCommentCommand> result = this._validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
