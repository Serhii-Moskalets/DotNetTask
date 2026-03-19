using FluentValidation;
using TodoListApp.Domain.Constants;
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
            .NotEmpty()
                .WithMessage(UserPolicy.IdRequiredMessage);

        this.RuleFor(x => x.CommentId)
            .NotEmpty()
                .WithMessage(CommentPolicy.IdRequiredMessage);

        this.RuleFor(x => x.NewContent)
            .NotEmpty()
                .WithMessage(CommentPolicy.EmptyMessage)
            .MaximumLength(CommentContent.MaxLength)
                .WithMessage(CommentPolicy.TooLongMessage);
    }
}
