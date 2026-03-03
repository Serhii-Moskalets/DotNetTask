using FluentValidation;

namespace TodoListApp.Application.Users.Commands.LoginUser;

/// <summary>
/// Validator for the <see cref="LoginUserCommand"/>.
/// </summary>
public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginUserCommandValidator"/> class.
    /// </summary>
    public LoginUserCommandValidator()
    {
        this.RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email cannot be null or empty.")
            .EmailAddress().WithMessage("Email address is incorrect.")
            .Matches("^[^<>]+$").WithMessage("Email address is incorrect.");

        this.RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
