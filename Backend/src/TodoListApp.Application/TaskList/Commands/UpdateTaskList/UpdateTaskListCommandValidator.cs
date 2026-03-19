using FluentValidation;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.TaskList.Commands.UpdateTaskList;

/// <summary>
/// Validator for <see cref="UpdateTaskListCommand"/>.
/// </summary>
public class UpdateTaskListCommandValidator : AbstractValidator<UpdateTaskListCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTaskListCommandValidator"/> class.
    /// </summary>
    public UpdateTaskListCommandValidator()
    {
        this.RuleFor(x => x.NewTitle)
            .NotEmpty()
                .WithMessage(TaskListPolicy.EmptyMessage)
            .MaximumLength(TaskListTitle.MaxLength)
                .WithMessage(TaskListPolicy.TooLongMessage);

        this.RuleFor(x => x.UserId)
            .NotEmpty()
                .WithMessage(UserPolicy.IdRequiredMessage);

        this.RuleFor(x => x.TaskListId)
            .NotEmpty()
                .WithMessage(TaskListPolicy.IdRequiredMessage);
    }
}
