using FluentValidation;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tasks.Queries.GetTaskByTitle;

/// <summary>
/// Validator for <see cref="GetTaskByTitleQuery"/>.
/// </summary>
public class GetTaskByTitleQueryValidator : AbstractValidator<GetTaskByTitleQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskByTitleQueryValidator"/> class.
    /// </summary>
    public GetTaskByTitleQueryValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(UserPolicy.IdRequiredMessage);

        this.RuleFor(x => x.Text)
            .MaximumLength(CommonPolicy.MaxSearchTextLength)
                .WithMessage(CommonPolicy.SearchTextTooLongMessage)
            .When(x => !string.IsNullOrWhiteSpace(x.Text));

        this.RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
                .WithMessage(CommonPolicy.PageMinMessage);

        this.RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
               .WithMessage(CommonPolicy.PageSizeRangeMessage);
    }
}
