using DotNetTask.Domain.Constants;
using DotNetTask.Domain.ValueObjects;

using FluentValidation;

using TinyResult.Enums;

namespace DotNetTask.Application.Users.Commands.UpdateUserProfile;

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
            .NotEmpty()
                .WithMessage(UserPolicy.IdRequiredMessage);

        this.RuleFor(x => x.FirstName)
            .MaximumLength(FirstName.MaxLength)
                .WithMessage(FirstNamePolicy.TooLongMessage)
            .When(x => x.FirstName != null);

        this.RuleFor(x => x.LastName)
            .MaximumLength(LastName.MaxLength)
                .WithMessage(LastNamePolicy.TooLongMessage)
            .When(x => x.LastName != null);

        this.RuleFor(x => x)
            .Must(command => !string.IsNullOrEmpty(command.FirstName) || !string.IsNullOrEmpty(command.LastName))
                .WithMessage(UserPolicy.AtLeastOneFieldRequiredMessage)
            .WithErrorCode(nameof(ErrorCode.ValidationError));
    }
}
