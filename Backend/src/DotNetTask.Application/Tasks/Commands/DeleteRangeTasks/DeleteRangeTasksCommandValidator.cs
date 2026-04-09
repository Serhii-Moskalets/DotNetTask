using DotNetTask.Domain.Constants;
using FluentValidation;

namespace DotNetTask.Application.Tasks.Commands.DeleteRangeTasks;

/// <summary>
/// Validator for <see cref="DeleteRangeTasksCommand"/>.
/// </summary>
public class DeleteRangeTasksCommandValidator : AbstractValidator<DeleteRangeTasksCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteRangeTasksCommandValidator"/> class.
    /// </summary>
    public DeleteRangeTasksCommandValidator()
    {
        this.RuleFor(x => x.TaskIds)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(TaskPolicy.EmptyTaskIdsCollectionMessage)
            .Must(ids => ids.Distinct().Count() == ids.Count()).WithMessage(TaskPolicy.DuplicateTaskIdsMessage);

        this.RuleForEach(x => x.TaskIds)
            .NotEmpty().WithMessage(TaskPolicy.EmptyTaskIdsCollectionMessage);

        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(UserPolicy.IdRequiredMessage);
    }
}
