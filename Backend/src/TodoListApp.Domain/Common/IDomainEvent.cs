using MediatR;

namespace TodoListApp.Domain.Common;

/// <summary>
/// Marker interface for domain events.
/// Inherits from INotification to be compatible with MediatR.
/// </summary>
public interface IDomainEvent : INotification;
