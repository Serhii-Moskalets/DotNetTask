using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;

namespace DotNetTask.Application.Common.Services;

/// <summary>
/// Provides methods to check if a user has access to a task.
/// Implements <see cref="ITaskAccessService"/>.
/// </summary>
public class TaskAccessService : ITaskAccessService
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskAccessService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to access repositories.</param>
    public TaskAccessService(IUnitOfWork unitOfWork) => this._unitOfWork = unitOfWork;

    /// <summary>
    /// Determines whether a specific user has access to a given task.
    /// A user has access if they are the owner of the task or if the task is shared with them.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <returns>A task that returns <c>true</c> if the user has access; otherwise, <c>false</c>.</returns>
    public async Task<bool> HasAccessAsync(Guid taskId, Guid userId, CancellationToken cancellationToken)
    {
        return await this._unitOfWork.Tasks.IsTaskOwnerAsync(taskId, userId, cancellationToken)
            || await this._unitOfWork.UserTaskAccesses.ExistsAsync(taskId, userId, cancellationToken);
    }
}
