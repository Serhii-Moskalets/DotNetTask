using DotNetTask.Domain.Constants;

using FluentValidation;

namespace DotNetTask.Application.UserTaskAccess.Commands.DeleteTaskAccessById;

/// <summary>
/// Validator for <see cref="DeleteTaskAccessByIdCommand"/>.
/// </summary>
public class DeleteTaskAccessByIdCommandValidator : AbstractValidator<DeleteTaskAccessByIdCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskAccessByIdCommandValidator"/> class.
    /// </summary>
    public DeleteTaskAccessByIdCommandValidator()
    {
        this.RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage(TaskPolicy.IdRequiredMessage);

        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(UserPolicy.IdRequiredMessage);

        this.RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage(UserTaskAccessPolicy.OwnerIdRequired);
    }
}
