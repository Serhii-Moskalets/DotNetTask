using FluentValidation;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.UserTaskAccess.Queries.GetSharedTasksByUserId;

/// <summary>
/// Validator for <see cref="GetSharedTasksByUserIdQuery"/>.
/// </summary>
public class GetSharedTasksByUserIdQueryValidator : AbstractValidator<GetSharedTasksByUserIdQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetSharedTasksByUserIdQueryValidator"/> class.
    /// </summary>
    public GetSharedTasksByUserIdQueryValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(UserPolicy.IdRequiredMessage);

        this.RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage(CommonPolicy.PageMinMessage);

        this.RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage(CommonPolicy.PageSizeRangeMessage);
    }
}
