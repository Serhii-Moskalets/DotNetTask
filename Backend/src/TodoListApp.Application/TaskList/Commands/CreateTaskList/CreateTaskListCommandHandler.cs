using MediatR;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Services;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.TaskList.Commands.CreateTaskList;

/// <summary>
/// Handles the <see cref="CreateTaskListCommand"/> by creating a new task list
/// for a specific user. If a task list with the same title already exists for
/// the user, a numeric suffix is appended to make the title unique.
/// </summary>
public class CreateTaskListCommandHandler(
    IUnitOfWork unitOfWork,
    IUniqueNameService uniqueNameService)
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
        var user = await this.UnitOfWork.Users.GetByIdAsync(command.UserId, asNoTracking: true, cancellationToken);
        if (user is null)
        {
            return await Result<Guid>.FailureAsync(ErrorCode.NotFound, "User not found.");
        }

        string uniqueTitle = await uniqueNameService.GetUniqueNameAsync(
            command.Title!,
            (name, ct) => this.UnitOfWork.TaskLists.ExistsByTitleAsync(name, command.UserId, ct),
            cancellationToken);

        var title = TaskListTitle.Create(uniqueTitle);

        var taskList = new TaskListEntity(user.Id, title);

        await this.UnitOfWork.TaskLists.AddAsync(taskList, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<Guid>.SuccessAsync(taskList.Id);
    }
}
