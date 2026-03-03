using MediatR;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessByUserEmail;

/// <summary>
/// Handles the deletion of a user-task access entry based on the task ID and the user's email.
/// </summary>
public class DeleteTaskAccessByUserEmailCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<DeleteTaskAccessByUserEmailCommand, Result<bool>>
{
    /// <summary>
    /// Handles the <see cref="DeleteTaskAccessByUserEmailCommand"/> by checking if the access exists,
    /// deleting it if present, and returning the operation result.
    /// </summary>
    /// <param name="command">The command containing the task ID and user email.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if the access was deleted,
    /// or failure if the access was not found.
    /// </returns>
    public async Task<Result<bool>> Handle(DeleteTaskAccessByUserEmailCommand command, CancellationToken cancellationToken)
    {
        var hasAccess = await this.UnitOfWork.Tasks.IsTaskOwnerAsync(command.TaskId, command.OwnerId, cancellationToken);
        if (!hasAccess)
        {
            return await Result<bool>.FailureAsync(ErrorCode.ValidationError, "User doesn't have access to this task.");
        }

        var sharedUser = await this.UnitOfWork.Users.GetByEmailAsync(
            Email.Create(command.Email),
            asNoTracking: true,
            cancellationToken);

        if (sharedUser is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.InvalidOperation, "Operation error.");
        }

        var deleted = await this.UnitOfWork.UserTaskAccesses.DeleteByIdAsync(command.TaskId, sharedUser!.Id, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        if (deleted <= 0)
        {
            return await Result<bool>.FailureAsync(ErrorCode.InvalidOperation, "Access wasn't deleted.");
        }

        return await Result<bool>.SuccessAsync(true);
    }
}
