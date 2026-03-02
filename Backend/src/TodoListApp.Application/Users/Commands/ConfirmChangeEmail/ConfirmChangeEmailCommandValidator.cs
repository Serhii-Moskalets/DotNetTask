using FluentValidation;

namespace TodoListApp.Application.Users.Commands.ConfirmChangeEmail;

/// <summary>
/// Validator for the <see cref="ConfirmChangeEmailCommand"/>.
/// </summary>
public class ConfirmChangeEmailCommandValidator : AbstractValidator<ConfirmChangeEmailCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmChangeEmailCommandValidator"/> class.
    /// </summary>
    public ConfirmChangeEmailCommandValidator()
    {
        this.RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.");
    }
}
