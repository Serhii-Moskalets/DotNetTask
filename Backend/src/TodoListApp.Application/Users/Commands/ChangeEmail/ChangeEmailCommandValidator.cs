using FluentValidation;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.ValueObjects;

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
            .NotEmpty().WithMessage(UserPolicy.IdRequiredMessage);

        this.RuleFor(x => x.NewEmail)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage(EmailPolicy.EmptyMessage)
            .MaximumLength(Email.MaxLength)
                .WithMessage(EmailPolicy.TooLongMessage)
            .Matches(EmailPolicy.FormatRegex)
                .WithMessage(EmailPolicy.InvalidFormatMessage);
    }
}
