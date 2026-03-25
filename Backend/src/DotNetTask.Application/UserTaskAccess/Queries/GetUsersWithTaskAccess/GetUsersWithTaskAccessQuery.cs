using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Application.UserTaskAccess.Dtos;

namespace DotNetTask.Application.UserTaskAccess.Queries.GetUsersWithTaskAccess;

/// <summary>
/// Represents a query to retrieve all users who have access to a specific task.
/// </summary>
public record GetUsersWithTaskAccessQuery(Guid TaskId, Guid UserId, int Page = 1, int PageSize = 10)
    : IQuery<TaskAccessListDto>;