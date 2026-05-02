namespace RGamaFelix.CqrsDispatcher.Event;

/// <summary>
///   Represents a domain event that can be published for concurrent asynchronous processing by multiple handlers.
///   Implementing this interface marks the derived type as an event, distinct from commands (single handler) and
///   queries (return a value).
/// </summary>
public interface IEvent : IRequestBase;
