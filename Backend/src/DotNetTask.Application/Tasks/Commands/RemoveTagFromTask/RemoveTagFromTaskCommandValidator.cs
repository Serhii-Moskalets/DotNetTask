using DotNetTask.Domain.Constants;

using FluentValidation;

namespace DotNetTask.Application.Tasks.Commands.RemoveTagFromTask;

/// <summary>
/// Validator for <see cref="RemoveTagFromTaskCommand"/>.
/// </summary>
public class RemoveTagFromTaskCommandValidator : AbstractValidator<RemoveTagFromTaskCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveTagFromTaskCommandValidator"/> class.
    /// </summary>
    public RemoveTagFromTaskCommandValidator()
    {
        this.RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage(TaskPolicy.IdRequiredMessage);

        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(UserPolicy.IdRequiredMessage);
    }
}
