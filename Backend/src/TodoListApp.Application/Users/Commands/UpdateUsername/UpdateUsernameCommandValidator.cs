using FluentValidation;

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
            .NotEmpty().WithMessage("Username is required.")
            .Length(3, 20).WithMessage("Username must be between 3 and 20 characters.");

        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
