using DotNetTask.Domain.Constants;
using FluentValidation;

namespace DotNetTask.Application.Users.Commands.InitiateAccountDeletion;

/// <summary>
/// Validator for the <see cref="InitiateAccountDeletionCommand"/>.
/// </summary>
public class InitiateAccountDeletionCommandValidator : AbstractValidator<InitiateAccountDeletionCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InitiateAccountDeletionCommandValidator"/> class.
    /// </summary>
    public InitiateAccountDeletionCommandValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(UserPolicy.IdRequiredMessage);
    }
}
