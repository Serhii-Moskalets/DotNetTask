using DotNetTask.Domain.Constants;

using FluentValidation;

namespace DotNetTask.Application.Users.Queries.GetUserProfile;

/// <summary>
/// Validator for <see cref="GetUserProfileQuery"/>.
/// </summary>
public class GetUserProfileQueryValidator : AbstractValidator<GetUserProfileQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserProfileQueryValidator"/> class.
    /// </summary>
    public GetUserProfileQueryValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(UserPolicy.IdRequiredMessage);
    }
}
