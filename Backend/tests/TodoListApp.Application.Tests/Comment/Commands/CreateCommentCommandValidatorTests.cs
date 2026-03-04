using FluentValidation.TestHelper;
using TodoListApp.Application.Comment.Commands.CreateComment;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Comment.Commands;

/// <summary>
/// Unit tests for <see cref="CreateCommentCommandValidator"/>.
/// Validates command properties and ensures correct validation messages.
/// </summary>
public class CreateCommentCommandValidatorTests
{
    private readonly CreateCommentCommandValidator _validator = new();

    /// <summary>
    /// Fails validation when UserId is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var command = new CreateCommentCommand(
            TaskId: Guid.NewGuid(),
            UserId: Guid.Empty,
            Content: "Some content");

        var result = this._validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.UserId)
              .WithErrorMessage("User ID is required.");
    }

    /// <summary>
    /// Fails validation when TaskId is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskId_Is_Empty()
    {
        var command = new CreateCommentCommand(
            TaskId: Guid.Empty,
            UserId: Guid.NewGuid(),
            Content: "Some content");

        var result = this._validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.TaskId)
              .WithErrorMessage("Task ID is required.");
    }

    /// <summary>
    /// Fails validation when Text is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_Text_Is_Empty()
    {
        var command = new CreateCommentCommand(
            TaskId: Guid.Empty,
            UserId: Guid.NewGuid(),
            Content: string.Empty);

        var result = this._validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Content)
              .WithErrorMessage("Comment content cannot be null or empty.");
    }

    /// <summary>
    /// Fails validation when Text exceeds maximum length.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_Text_Exceeds_MaxLength()
    {
        var longText = new string('a', CommentContent.MaxLength + 1);
        var command = new CreateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), longText);

        var result = this._validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Content)
              .WithErrorMessage($"Comment content cannot exceed {CommentContent.MaxLength} characters.");
    }

    /// <summary>
    /// Passes validation when command is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new CreateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), "Valid comment");

        var result = this._validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
