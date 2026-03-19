using FluentValidation;
using TodoListApp.Domain.Constants;
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
            .NotEmpty().WithMessage(TaskListPolicy.IdRequiredMessage);

        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(UserPolicy.IdRequiredMessage);

        this.RuleFor(x => x.Dto.Title)
            .NotEmpty().
                WithMessage(TaskPolicy.EmptyTitleMessage)
            .MaximumLength(TaskTitle.MaxLength)
                .WithMessage(TaskPolicy.TooLongTitleMessage);

        this.RuleFor(x => x.Dto.DueDate)
            .Must((dueDate) => dueDate == null || dueDate.Value >= DateTime.UtcNow)
                .WithMessage(TaskPolicy.InvalidDueDateMessage);
    }
}
