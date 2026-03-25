using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Application.Common.Dtos;

namespace DotNetTask.Application.Tasks.Queries.GetTaskById;

/// <summary>
/// Query to retrieve a specific task by its ID for a given user.
/// </summary>
public sealed record GetTaskByIdQuery(Guid UserId, Guid TaskId)
    : IQuery<TaskDto>;
