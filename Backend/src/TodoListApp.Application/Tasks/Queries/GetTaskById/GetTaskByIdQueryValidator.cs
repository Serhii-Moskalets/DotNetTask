using FluentValidation;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tasks.Queries.GetTaskById;

/// <summary>
/// Validator for <see cref="GetTaskByIdQuery"/>.
/// </summary>
public class GetTaskByIdQueryValidator : AbstractValidator<GetTaskByIdQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskByIdQueryValidator"/> class.
    /// </summary>
    public GetTaskByIdQueryValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(UserPolicy.IdRequiredMessage);

        this.RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage(TaskPolicy.IdRequiredMessage);
    }
}
