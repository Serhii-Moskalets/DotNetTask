using FluentValidation;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tasks.Commands.UpdateTask;

/// <summary>
/// Validator for <see cref="UpdateTaskCommand"/>.
/// Ensures that the command contains valid data before it is processed.
/// </summary>
public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTaskCommandValidator"/> class
    /// and sets up validation rules for updating a task.
    /// </summary>
    public UpdateTaskCommandValidator()
    {
        this.RuleFor(x => x.Dto.TaskId)
            .NotEmpty().WithMessage(TaskPolicy.IdRequiredMessage);

        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(UserPolicy.IdRequiredMessage);

        this.RuleFor(x => x.Dto.Title)
            .MaximumLength(TaskTitle.MaxLength)
                .WithMessage(TaskPolicy.TooLongTitleMessage);

        this.RuleFor(x => x.Dto.Description)
            .MaximumLength(TaskDescription.MaxLength)
                .WithMessage(TaskPolicy.TooLongDescriptionMessage);

        this.RuleFor(x => x.Dto.DueDate)
            .Must((dueDate) => dueDate == null || dueDate.Value >= DateTime.UtcNow)
                .WithMessage(TaskPolicy.InvalidDueDateMessage);
    }
}
