using DotNetTask.Domain.Constants;
using DotNetTask.Domain.ValueObjects;

using FluentValidation;

namespace DotNetTask.Application.Users.Commands.ResetPassword;

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
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage(EmailPolicy.EmptyMessage)
            .MaximumLength(Email.MaxLength)
                .WithMessage(EmailPolicy.TooLongMessage)
            .Matches(EmailPolicy.FormatRegex)
                .WithMessage(EmailPolicy.InvalidFormatMessage);
    }
}
