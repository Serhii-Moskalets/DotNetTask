using DotNetTask.Domain.Constants;
using DotNetTask.Domain.ValueObjects;

using FluentValidation;

namespace DotNetTask.Application.TaskList.Commands.CreateTaskList;

/// <summary>
/// Validates the <see cref="CreateTaskListCommand"/> to ensure all required properties
/// meet the defined business rules.
/// </summary>
public class CreateTaskListCommandValidator : AbstractValidator<CreateTaskListCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTaskListCommandValidator"/> class.
    /// </summary>
    public CreateTaskListCommandValidator()
    {
        this.RuleFor(x => x.Title)
            .NotEmpty()
                .WithMessage(TaskListPolicy.EmptyMessage)
            .MaximumLength(TaskListTitle.MaxLength)
                .WithMessage(TaskListPolicy.TooLongMessage);

        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(UserPolicy.IdRequiredMessage);
    }
}
