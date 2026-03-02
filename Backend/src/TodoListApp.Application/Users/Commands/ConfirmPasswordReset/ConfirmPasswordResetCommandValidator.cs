using FluentValidation;

namespace TodoListApp.Application.Users.Commands.ConfirmPasswordReset;

/// <summary>
/// Validator for the <see cref="ConfirmPasswordResetCommand"/>.
/// </summary>
public class ConfirmPasswordResetCommandValidator : AbstractValidator<ConfirmPasswordResetCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmPasswordResetCommandValidator"/> class.
    /// </summary>
    public ConfirmPasswordResetCommandValidator()
    {
        this.RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.");

        this.RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches(@"[\!\?\*\.]").WithMessage("Password must contain at least one special character (!?*.).");
    }
}
