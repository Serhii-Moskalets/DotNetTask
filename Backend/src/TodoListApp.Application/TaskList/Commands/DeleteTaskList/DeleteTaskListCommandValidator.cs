using FluentValidation;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.TaskList.Commands.DeleteTaskList;

/// <summary>
/// Validator for <see cref="DeleteTaskListCommand"/>.
/// </summary>
public class DeleteTaskListCommandValidator : AbstractValidator<DeleteTaskListCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskListCommandValidator"/> class.
    /// </summary>
    public DeleteTaskListCommandValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(UserPolicy.UserIdRequiredMessage);

        this.RuleFor(x => x.TaskListId)
            .NotEmpty().WithMessage(TaskListPolicy.IdRequiredMessage);
    }
}
