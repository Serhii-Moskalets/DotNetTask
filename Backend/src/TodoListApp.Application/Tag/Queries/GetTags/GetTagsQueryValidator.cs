using FluentValidation;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tag.Queries.GetTags;

/// <summary>
/// Validator for <see cref="GetTagsQuery"/>.
/// </summary>
public class GetTagsQueryValidator : AbstractValidator<GetTagsQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTagsQueryValidator"/> class.
    /// </summary>
    public GetTagsQueryValidator()
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
