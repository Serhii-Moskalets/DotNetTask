using DotNetTask.Domain.Constants;
using DotNetTask.Domain.ValueObjects;

using FluentValidation;

namespace DotNetTask.Application.Tag.Commands.CreateTag;

/// <summary>
/// Validator for <see cref="CreateTagCommand"/>.
/// </summary>
public class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTagCommandValidator"/> class.
    /// </summary>
    public CreateTagCommandValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty()
                .WithMessage(UserPolicy.IdRequiredMessage);

        this.RuleFor(x => x.TaskId)
            .NotEmpty()
                .WithMessage(TaskPolicy.IdRequiredMessage);

        this.RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage(TagPolicy.EmptyMessage)
            .MaximumLength(TagName.MaxLength)
                .WithMessage(TagPolicy.TooLongMessage);
    }
}
