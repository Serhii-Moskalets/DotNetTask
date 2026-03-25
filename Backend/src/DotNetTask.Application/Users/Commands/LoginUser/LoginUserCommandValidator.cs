using DotNetTask.Domain.Constants;
using DotNetTask.Domain.ValueObjects;

using FluentValidation;

namespace DotNetTask.Application.Users.Commands.LoginUser;

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
            .NotEmpty()
                .WithMessage(EmailPolicy.EmptyMessage)
            .EmailAddress()
                .WithMessage(EmailPolicy.InvalidFormatMessage)
            .MaximumLength(Email.MaxLength)
                .WithMessage(EmailPolicy.TooLongMessage)
            .Matches(EmailPolicy.FormatRegex)
                .WithMessage(EmailPolicy.InvalidFormatMessage);

        this.RuleFor(x => x.Password)
            .NotEmpty().WithMessage(PasswordPolicy.EmptyMessage);
    }
}
