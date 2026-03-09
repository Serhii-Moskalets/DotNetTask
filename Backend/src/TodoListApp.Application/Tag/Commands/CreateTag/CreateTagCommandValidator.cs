using FluentValidation;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tag.Commands.CreateTag;

/// <summary>
/// Validator for <see cref="CreateTagCommand"/>.
/// </summary>
public class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTagCommandValidator"/> class.
    /// </summary>
    public CreateTagCommandValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        this.RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required.");

        this.RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tag name cannot be null or empty.")
            .MaximumLength(TagName.MaxLength).WithMessage($"Tag name cannot exceed {TagName.MaxLength} characters.");
    }
}
