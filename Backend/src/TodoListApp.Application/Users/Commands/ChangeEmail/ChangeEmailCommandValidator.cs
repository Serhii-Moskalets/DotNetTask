using FluentValidation;

namespace TodoListApp.Application.Users.Commands.ChangeEmail;

/// <summary>
/// Validator for the <see cref="ChangeEmailCommand"/>.
/// </summary>
public class ChangeEmailCommandValidator : AbstractValidator<ChangeEmailCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeEmailCommandValidator"/> class.
    /// </summary>
    public ChangeEmailCommandValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        this.RuleFor(x => x.NewEmail)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}
