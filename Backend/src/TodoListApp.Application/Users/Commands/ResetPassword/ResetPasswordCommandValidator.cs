using FluentValidation;

namespace TodoListApp.Application.Users.Commands.ResetPassword;

/// <summary>
/// Validator for the <see cref="ResetPasswordCommand"/>.
/// </summary>
public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResetPasswordCommandValidator"/> class.
    /// </summary>
    public ResetPasswordCommandValidator()
    {
        this.RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.");
    }
}
