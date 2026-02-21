using FluentValidation;

namespace TodoListApp.Application.Users.Commands.RevertEmailChange;

/// <summary>
/// Validator for the <see cref="RevertEmailChangeCommand"/>.
/// </summary>
public class RevertEmailChangeCommandValidator : AbstractValidator<RevertEmailChangeCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RevertEmailChangeCommandValidator"/> class.
    /// </summary>
    public RevertEmailChangeCommandValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        this.RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.");
    }
}
