using DotNetTask.Domain.Constants;

using FluentValidation;

namespace DotNetTask.Application.Comment.Commands.DeleteComment;

/// <summary>
/// Validator for <see cref="DeleteCommentCommand"/>.
/// </summary>
public class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteCommentCommandValidator"/> class.
    /// </summary>
    public DeleteCommentCommandValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(UserPolicy.IdRequiredMessage);

        this.RuleFor(x => x.CommentId)
            .NotEmpty().WithMessage(TaskPolicy.IdRequiredMessage);
    }
}
