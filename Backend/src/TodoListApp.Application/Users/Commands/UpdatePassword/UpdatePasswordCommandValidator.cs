using FluentValidation;

namespace TodoListApp.Application.Users.Commands.UpdatePassword;

/// <summary>
/// Validator for the <see cref="UpdatePasswordCommand"/>.
/// </summary>
public class UpdatePasswordCommandValidator : AbstractValidator<UpdatePasswordCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePasswordCommandValidator"/> class.
    /// </summary>
    public UpdatePasswordCommandValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        this.RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        this.RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches(@"[\!\?\*\.]").WithMessage("Password must contain at least one special character (!?*.).")
            .NotEqual(x => x.CurrentPassword).WithMessage("New password and current password cannot be the same.");
    }
}
