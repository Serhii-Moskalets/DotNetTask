using FluentValidation;
using TinyResult.Enums;

namespace TodoListApp.Application.Users.Commands.UpdateUserProfile;

/// <summary>
/// Validator for the <see cref="UpdateUserProfileCommand"/>.
/// </summary>
public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserProfileCommandValidator"/> class.
    /// </summary>
    public UpdateUserProfileCommandValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        this.RuleFor(x => x.FirstName)
            .MaximumLength(20).WithMessage("First name cannot be longer than 20 characters.")
            .When(x => x.FirstName != null);

        this.RuleFor(x => x.LastName)
            .MaximumLength(30).WithMessage("Last name cannot be longer than 30 characters.")
            .When(x => x.LastName != null);

        this.RuleFor(x => x)
            .Must(command => !string.IsNullOrEmpty(command.FirstName) || !string.IsNullOrEmpty(command.LastName))
            .WithMessage("At least one field (FirstName or LastName) must be provided.")
            .WithErrorCode(nameof(ErrorCode.ValidationError));
    }
}
