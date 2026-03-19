using FluentValidation;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tasks.Commands.DeleteOverdueTasks;

/// <summary>
/// Validator for <see cref="DeleteOverdueTasksCommand"/>.
/// </summary>
public class DeleteOverdueTasksCommandValidator : AbstractValidator<DeleteOverdueTasksCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteOverdueTasksCommandValidator"/> class.
    /// </summary>
    public DeleteOverdueTasksCommandValidator()
    {
        this.RuleFor(x => x.TaskListId)
            .NotEmpty().WithMessage(TaskListPolicy.IdRequiredMessage);

        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(UserPolicy.IdRequiredMessage);
    }
}
