using DotNetTask.Domain.Constants;

using FluentValidation;

namespace DotNetTask.Application.Users.Commands.ConfirmEmail;

/// <summary>
/// Validator for the <see cref="ConfirmEmailCommand"/>.
/// </summary>
public class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmEmailCommandValidator"/> class.
    /// </summary>
    public ConfirmEmailCommandValidator()
    {
        this.RuleFor(x => x.Token)
            .NotEmpty().WithMessage(TokenPolicy.RequiredMessage);
    }
}
