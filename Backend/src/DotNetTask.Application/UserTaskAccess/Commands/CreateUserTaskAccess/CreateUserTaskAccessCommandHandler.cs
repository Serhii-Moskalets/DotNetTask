using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using MediatR;

using TinyResult;

using Unit = DotNetTask.Domain.Common.Unit;

namespace DotNetTask.Application.UserTaskAccess.Commands.CreateUserTaskAccess;

/// <summary>
/// Handles the creation of a user-task access relationship.
/// </summary>
public class CreateUserTaskAccessCommandHandler(
    IUnitOfWork unitOfWork,
    IUserTaskAccessService userTaskAccessService)
    : HandlerBase(unitOfWork), IRequestHandler<CreateUserTaskAccessCommand, Result<Unit>>
{
    private readonly IUserTaskAccessService _userTaskAccessService = userTaskAccessService;

    /// <summary>
    /// Handles the <see cref="CreateUserTaskAccessCommand"/> to grant a user access to a task.
    /// </summary>
    /// <param name="command">The command containing the task ID and user email.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if the access was created,
    /// or failure if the user does not exist, is the task owner, or already has access.
    /// </returns>
    public async Task<Result<Unit>> Handle(CreateUserTaskAccessCommand command, CancellationToken cancellationToken)
    {
        UserEntity? sharedUser = await this.UnitOfWork.Users.GetByEmailAsync(
            Email.Create(command.Email),
            asNoTracking: true,
            cancellationToken);

        Result<Unit> accessValidation = await this._userTaskAccessService
            .CanGrantAccessAsync(command.TaskId, command.OwnerId, sharedUser, cancellationToken);

        if (!accessValidation.IsSuccess)
        {
            return accessValidation;
        }

        UserTaskAccessEntity access = new(command.TaskId, sharedUser!.Id);

        await this.UnitOfWork.UserTaskAccesses.AddAsync(access, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
