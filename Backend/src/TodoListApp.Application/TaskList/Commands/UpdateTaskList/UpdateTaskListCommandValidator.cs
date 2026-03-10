using FluentValidation;
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
            .NotEmpty().WithMessage("New title cannot be null or empty.")
            .MaximumLength(TaskListTitle.MaxLength).WithMessage($"Title cannot exceed {TaskListTitle.MaxLength} characters.");

        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        this.RuleFor(x => x.TaskListId)
            .NotEmpty().WithMessage("Task list ID is required.");
    }
}
