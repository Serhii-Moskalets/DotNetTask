using FluentValidation;
using TodoListApp.Domain.Constants;

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
            .NotEmpty()
                .WithMessage(UserPolicy.UserIdRequiredMessage);

        this.RuleFor(x => x.CurrentPassword)
            .NotEmpty()
                .WithMessage(PasswordPolicy.EmptyMessage);

        this.RuleFor(x => x.NewPassword)
            .Cascade(CascadeMode.Stop)
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
                .WithMessage(PasswordPolicy.SpecialCharMessage)
            .NotEqual(x => x.CurrentPassword)
                .WithMessage(PasswordPolicy.SameAsOldMessage);
    }
}
