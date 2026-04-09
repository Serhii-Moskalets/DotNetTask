using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using MediatR;

using TinyResult;
using TinyResult.Enums;

using Unit = DotNetTask.Domain.Common.Unit;

namespace DotNetTask.Application.UserTaskAccess.Commands.DeleteTaskAccessByUserEmail;

/// <summary>
/// Handles the deletion of a user-task access entry based on the task ID and the user's email.
/// </summary>
public class DeleteTaskAccessByUserEmailCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<DeleteTaskAccessByUserEmailCommand, Result<Unit>>
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
    public async Task<Result<Unit>> Handle(DeleteTaskAccessByUserEmailCommand command, CancellationToken cancellationToken)
    {
        bool hasAccess = await this.UnitOfWork.Tasks.IsTaskOwnerAsync(command.TaskId, command.OwnerId, cancellationToken);
        if (!hasAccess)
        {
            return Result<Unit>.Failure(ErrorCode.ValidationError, UserTaskAccessPolicy.AccessDeniedMessage);
        }

        UserEntity? sharedUser = await this.UnitOfWork.Users.GetByEmailAsync(
            Email.Create(command.Email),
            asNoTracking: true,
            cancellationToken);

        if (sharedUser is null)
        {
            return Result<Unit>.Failure(ErrorCode.InvalidOperation, UserTaskAccessPolicy.UserNotFoundMessage);
        }

        int deleted = await this.UnitOfWork.UserTaskAccesses.DeleteByIdAsync(command.TaskId, sharedUser!.Id, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        if (deleted <= 0)
        {
            return Result<Unit>.Failure(ErrorCode.InvalidOperation, UserTaskAccessPolicy.DeleteFailedMessage);
        }

        return Result<Unit>.Success(Unit.Value);
    }
}
