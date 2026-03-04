using FluentValidation;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Comment.Commands.CreateComment;

/// <summary>
/// Validator for <see cref="CreateCommentCommand"/>.
/// </summary>
public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCommentCommandValidator"/> class.
    /// </summary>
    public CreateCommentCommandValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        this.RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required.");

        this.RuleFor(c => c.Content)
            .NotEmpty().WithMessage("Comment content cannot be null or empty.")
            .MaximumLength(CommentContent.MaxLength).WithMessage($"Comment content cannot exceed {CommentContent.MaxLength} characters.");
    }
}
