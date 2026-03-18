using FluentValidation;
using TodoListApp.Domain.Constants;
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
            .NotEmpty()
            .WithMessage(UserPolicy.UserIdRequiredMessage);

        this.RuleFor(x => x.TaskId)
            .NotEmpty()
                .WithMessage(TaskPolicy.IdRequiredMessage);

        this.RuleFor(c => c.Content)
            .NotEmpty()
                .WithMessage(CommentPolicy.EmptyMessage)
            .MaximumLength(CommentContent.MaxLength)
                .WithMessage(CommentPolicy.TooLongMessage);
    }
}
