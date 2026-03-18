using FluentValidation.TestHelper;
using TodoListApp.Application.Comment.Commands.DeleteComment;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tests.Comment.Commands;

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
        var command = new DeleteCommentCommand(Guid.NewGuid(), Guid.Empty);

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
        var command = new DeleteCommentCommand(Guid.Empty, Guid.NewGuid());

        var result = this._validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.CommentId)
              .WithErrorMessage(TaskPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Passes validation when command is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new DeleteCommentCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = this._validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
