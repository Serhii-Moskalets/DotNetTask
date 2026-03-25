using DotNetTask.Domain.Constants;

using FluentValidation;

namespace DotNetTask.Application.UserTaskAccess.Commands.DeleteTaskAccessesByTask;

/// <summary>
/// Validator for <see cref="DeleteTaskAccessesByTaskCommand"/>.
/// </summary>
public class DeleteTaskAccessesByTaskCommandValidator
    : AbstractValidator<DeleteTaskAccessesByTaskCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskAccessesByTaskCommandValidator"/> class.
    /// </summary>
    public DeleteTaskAccessesByTaskCommandValidator()
    {
        this.RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage(TaskPolicy.IdRequiredMessage);
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(UserPolicy.IdRequiredMessage);
    }
}
