using DotNetTask.Application.Comment.Commands.CreateComment;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.ValueObjects;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Comment.Commands;

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
        // Arrange
        CreateCommentCommand command = new(
            TaskId: Guid.NewGuid(),
            UserId: Guid.Empty,
            Content: "Some content");

        // Act
        TestValidationResult<CreateCommentCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Fails validation when TaskId is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskId_Is_Empty()
    {
        // Arrange
        CreateCommentCommand command = new(
            TaskId: Guid.Empty,
            UserId: Guid.NewGuid(),
            Content: "Some content");

        // Act
        TestValidationResult<CreateCommentCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.TaskId)
              .WithErrorMessage(TaskPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Fails validation when Text is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_Text_Is_Empty()
    {
        // Arrange
        CreateCommentCommand command = new(
            TaskId: Guid.Empty,
            UserId: Guid.NewGuid(),
            Content: string.Empty);

        // Act
        TestValidationResult<CreateCommentCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Content)
              .WithErrorMessage(CommentPolicy.EmptyMessage);
    }

    /// <summary>
    /// Fails validation when Text exceeds maximum length.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_Text_Exceeds_MaxLength()
    {
        // Arrange
        string longText = new('a', CommentContent.MaxLength + 1);
        CreateCommentCommand command = new(Guid.NewGuid(), Guid.NewGuid(), longText);

        // Act
        TestValidationResult<CreateCommentCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Content)
              .WithErrorMessage(CommentPolicy.TooLongMessage);
    }

    /// <summary>
    /// Passes validation when command is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        CreateCommentCommand command = new(Guid.NewGuid(), Guid.NewGuid(), "Valid comment");

        // Act
        TestValidationResult<CreateCommentCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
