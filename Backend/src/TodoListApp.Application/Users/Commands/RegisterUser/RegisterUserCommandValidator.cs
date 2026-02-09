using FluentValidation;

namespace TodoListApp.Application.Users.Commands.RegisterUser;

/// <summary>
/// Validator for the <see cref="RegisterUserCommand"/>.
/// </summary>
public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterUserCommandValidator"/> class.
    /// </summary>
    public RegisterUserCommandValidator()
    {
        this.RuleFor(x => x.FirstName)
            .NotEmpty().MaximumLength(20);

        this.RuleFor(x => x.UserName)
            .NotEmpty().MinimumLength(3).MaximumLength(20);

        this.RuleFor(x => x.Email)
            .NotEmpty().EmailAddress();

        this.RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches(@"[\!\?\*\.]").WithMessage("Password must contain at least one special character (!?*.).");
    }
}
