using FluentValidation;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Users.Commands.UpdateUsername;

/// <summary>
/// Validator for <see cref="UpdateUsernameCommand"/>.
/// Ensures that the command contains valid data before it is processed.
/// </summary>
public class UpdateUsernameCommandValidator : AbstractValidator<UpdateUsernameCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUsernameCommandValidator"/> class.
    /// </summary>
    public UpdateUsernameCommandValidator()
    {
        this.RuleFor(x => x.UserName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage(UserNamePolicy.EmptyMessage)
            .Length(UserName.MinLength, UserName.MaxLength)
                .WithMessage(UserNamePolicy.LengthMessage)
            .Matches(UserNamePolicy.FormatRegex)
                .WithMessage(UserNamePolicy.InvalidCharactersMessage);

        this.RuleFor(x => x.UserId)
            .NotEmpty()
                .WithMessage(UserPolicy.UserIdRequiredMessage);
    }
}
