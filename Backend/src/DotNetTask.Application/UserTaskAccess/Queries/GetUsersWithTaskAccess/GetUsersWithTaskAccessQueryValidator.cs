using DotNetTask.Domain.Constants;

using FluentValidation;

namespace DotNetTask.Application.UserTaskAccess.Queries.GetUsersWithTaskAccess;

/// <summary>
/// Validator for <see cref="GetUsersWithTaskAccessQuery"/>.
/// </summary>
public class GetUsersWithTaskAccessQueryValidator : AbstractValidator<GetUsersWithTaskAccessQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetUsersWithTaskAccessQueryValidator"/> class.
    /// </summary>
    public GetUsersWithTaskAccessQueryValidator()
    {
        this.RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage(TaskPolicy.IdRequiredMessage);

        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(UserPolicy.IdRequiredMessage);

        this.RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage(CommonPolicy.PageMinMessage);

        this.RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage(CommonPolicy.PageSizeRangeMessage);
    }
}
