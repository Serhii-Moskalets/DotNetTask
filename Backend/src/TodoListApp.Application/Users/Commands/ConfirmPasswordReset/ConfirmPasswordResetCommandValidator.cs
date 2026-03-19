using FluentValidation;
using TodoListApp.Domain.Constants;

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
            .NotEmpty().WithMessage(TokenPolicy.RequiredMessage);

        this.RuleFor(x => x.NewPassword)
            .NotEmpty()
                .WithMessage(PasswordPolicy.EmptyMessage)
            .MinimumLength(PasswordPolicy.MinLength)
                .WithMessage(PasswordPolicy.TooShortMessage)
            .Matches(PasswordPolicy.UppercaseRegex)
                .WithMessage(PasswordPolicy.UppercaseMessage)
            .Matches(PasswordPolicy.LowercaseRegex)
                .WithMessage(PasswordPolicy.LowercaseMessage)
            .Matches(PasswordPolicy.NumberRegex)
                .WithMessage(PasswordPolicy.NumberMessage)
            .Matches(PasswordPolicy.SpecialCharRegex)
                .WithMessage(PasswordPolicy.SpecialCharMessage);
    }
}