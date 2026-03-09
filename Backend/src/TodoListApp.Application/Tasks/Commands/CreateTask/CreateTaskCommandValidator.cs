using FluentValidation;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tasks.Commands.CreateTask;

/// <summary>
/// Validator for <see cref="CreateTaskCommand"/>.
/// </summary>
public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTaskCommandValidator"/> class.
    /// </summary>
    public CreateTaskCommandValidator()
    {
        this.RuleFor(x => x.Dto.TaskListId)
            .NotEmpty().WithMessage("Task list ID is required.");

        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        this.RuleFor(x => x.Dto.Title)
            .NotEmpty().WithMessage("Task title cannot be empty.")
            .MaximumLength(TaskTitle.MaxLength).WithMessage($"Title cannot exceed {TaskTitle.MaxLength} characters.");

        this.RuleFor(x => x.Dto.DueDate)
            .Must((dueDate) => dueDate == null || dueDate.Value >= DateTime.UtcNow)
            .WithMessage("Due date cannot be in the past.");
    }
}
