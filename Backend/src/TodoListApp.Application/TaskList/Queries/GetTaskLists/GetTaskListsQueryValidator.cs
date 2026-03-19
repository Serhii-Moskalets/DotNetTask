using FluentValidation;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.TaskList.Queries.GetTaskLists;

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
                .WithMessage(UserPolicy.UserIdRequiredMessage);

        this.RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
                .WithMessage(PaginationPolicy.PageMinMessage);

        this.RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                    .WithMessage(PaginationPolicy.PageSizeRangeMessage);
    }
}
