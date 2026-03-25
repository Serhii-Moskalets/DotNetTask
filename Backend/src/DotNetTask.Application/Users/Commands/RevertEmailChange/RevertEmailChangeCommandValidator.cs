using DotNetTask.Domain.Constants;

using FluentValidation;

namespace DotNetTask.Application.Users.Commands.RevertEmailChange;

/// <summary>
/// Validator for the <see cref="RevertEmailChangeCommand"/>.
/// </summary>
public class RevertEmailChangeCommandValidator : AbstractValidator<RevertEmailChangeCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RevertEmailChangeCommandValidator"/> class.
    /// </summary>
    public RevertEmailChangeCommandValidator()
    {
        this.RuleFor(x => x.Token)
            .NotEmpty().WithMessage(TokenPolicy.RequiredMessage);
    }
}
