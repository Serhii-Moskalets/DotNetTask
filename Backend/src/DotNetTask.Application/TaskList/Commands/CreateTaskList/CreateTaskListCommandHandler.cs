using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using MediatR;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.TaskList.Commands.CreateTaskList;

/// <summary>
/// Handles the <see cref="CreateTaskListCommand"/> by creating a new task list
/// for a specific user. If a task list with the same title already exists for
/// the user, a numeric suffix is appended to make the title unique.
/// </summary>
public class CreateTaskListCommandHandler(
    IUnitOfWork unitOfWork,
    IUniqueValueService uniqueNameService)
    : HandlerBase(unitOfWork), IRequestHandler<CreateTaskListCommand, Result<Guid>>
{
    /// <summary>
    /// Processes the command to create a new task list.
    /// </summary>
    /// <param name="command">The command containing the user ID and title for the new task list.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Result{T}"/> indicating whether the operation was successful.</returns>
    public async Task<Result<Guid>> Handle(CreateTaskListCommand command, CancellationToken cancellationToken)
    {
        UserEntity? user = await this.UnitOfWork.Users.GetByIdAsync(command.UserId, asNoTracking: true, cancellationToken);
        if (user is null)
        {
            return Result<Guid>.Failure(ErrorCode.NotFound, UserPolicy.AccountNotFoundMessage);
        }

        TaskListTitle title = await uniqueNameService.GetUniqueValueAsync(
            command.Title,
            name => TaskListTitle.Create(name),
            (vo, ct) => this.UnitOfWork.TaskLists.ExistsByTitleAsync(vo, command.UserId, ct),
            cancellationToken);

        TaskListEntity taskList = new(user.Id, title);

        await this.UnitOfWork.TaskLists.AddAsync(taskList, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(taskList.Id);
    }
}
