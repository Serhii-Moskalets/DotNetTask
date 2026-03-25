using DotNetTask.Domain.Constants;

using FluentValidation;

namespace DotNetTask.Application.Tag.Commands.DeleteTag;

/// <summary>
/// Validator for <see cref="DeleteTagCommand"/>.
/// </summary>
public class DeleteTagCommandValidator : AbstractValidator<DeleteTagCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTagCommandValidator"/> class.
    /// </summary>
    public DeleteTagCommandValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty()
                .WithMessage(UserPolicy.IdRequiredMessage);

        this.RuleFor(x => x.TagId)
            .NotEmpty()
                .WithMessage(TagPolicy.IdRequiredMessage);
    }
}
