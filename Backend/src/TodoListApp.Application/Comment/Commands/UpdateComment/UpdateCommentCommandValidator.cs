using FluentValidation;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Comment.Commands.UpdateComment;

/// <summary>
/// Validator for <see cref="UpdateCommentCommand"/>.
/// </summary>
public class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCommentCommandValidator"/> class.
    /// </summary>
    public UpdateCommentCommandValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        this.RuleFor(x => x.CommentId)
            .NotEmpty().WithMessage("Comment ID is required.");

        this.RuleFor(x => x.NewContent)
            .NotEmpty().WithMessage("New content cannot be null or empty.")
            .MaximumLength(CommentContent.MaxLength).WithMessage($"Comment content cannot exceed {CommentContent.MaxLength} characters.");
    }
}
