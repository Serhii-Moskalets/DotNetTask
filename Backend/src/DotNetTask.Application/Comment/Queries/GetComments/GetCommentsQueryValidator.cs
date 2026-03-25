using DotNetTask.Domain.Constants;

using FluentValidation;

namespace DotNetTask.Application.Comment.Queries.GetComments;

/// <summary>
/// Validator for <see cref="GetCommentsQuery"/>.
/// </summary>
public class GetCommentsQueryValidator : AbstractValidator<GetCommentsQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetCommentsQueryValidator"/> class.
    /// </summary>
    public GetCommentsQueryValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty()
                .WithMessage(UserPolicy.IdRequiredMessage);

        this.RuleFor(x => x.TaskId)
            .NotEmpty()
                .WithMessage(TaskPolicy.IdRequiredMessage);

        this.RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
                .WithMessage(CommonPolicy.PageMinMessage);

        this.RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                    .WithMessage(CommonPolicy.PageSizeRangeMessage);
    }
}
