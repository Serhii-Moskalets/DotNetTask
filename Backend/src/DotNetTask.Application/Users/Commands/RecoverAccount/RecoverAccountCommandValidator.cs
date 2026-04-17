using DotNetTask.Domain.Constants;
using FluentValidation;

namespace DotNetTask.Application.Users.Commands.RecoverAccount;

/// <summary>
/// Validator for the <see cref="RecoverAccountCommand"/>.
/// </summary>
public class RecoverAccountCommandValidator : AbstractValidator<RecoverAccountCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverAccountCommandValidator"/> class.
    /// </summary>
    public RecoverAccountCommandValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(UserPolicy.IdRequiredMessage);
    }
}
