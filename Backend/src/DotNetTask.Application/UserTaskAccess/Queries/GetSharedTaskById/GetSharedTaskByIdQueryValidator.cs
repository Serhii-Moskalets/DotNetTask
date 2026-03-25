using DotNetTask.Domain.Constants;

using FluentValidation;

namespace DotNetTask.Application.UserTaskAccess.Queries.GetSharedTaskById;

/// <summary>
/// Validator for <see cref="GetSharedTaskByIdQuery"/>.
/// </summary>
public class GetSharedTaskByIdQueryValidator : AbstractValidator<GetSharedTaskByIdQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetSharedTaskByIdQueryValidator"/> class.
    /// </summary>
    public GetSharedTaskByIdQueryValidator()
    {
        this.RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage(TaskPolicy.IdRequiredMessage);
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(UserPolicy.IdRequiredMessage);
    }
}
