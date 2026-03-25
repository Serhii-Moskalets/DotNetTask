using DotNetTask.Domain.Constants;

using FluentValidation;

namespace DotNetTask.Application.TaskList.Queries.GetTaskLists;

/// <summary>
/// Validator for <see cref="GetTaskListsQuery"/>.
/// </summary>
public class GetTaskListsQueryValidator : AbstractValidator<GetTaskListsQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskListsQueryValidator"/> class.
    /// </summary>
    public GetTaskListsQueryValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty()
                .WithMessage(UserPolicy.IdRequiredMessage);

        this.RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
                .WithMessage(CommonPolicy.PageMinMessage);

        this.RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                    .WithMessage(CommonPolicy.PageSizeRangeMessage);
    }
}
