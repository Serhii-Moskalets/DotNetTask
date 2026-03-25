using DotNetTask.Domain.Constants;
using DotNetTask.Domain.ValueObjects;

using FluentValidation;

namespace DotNetTask.Application.Comment.Commands.UpdateComment;

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
