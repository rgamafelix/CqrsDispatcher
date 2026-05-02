namespace RGamaFelix.CqrsDispatcher.Event.Handler;

/// <summary>Defines a contract for handling a specific event type.</summary>
/// <typeparam name="TEvent">The type of the event. Must implement <see cref="IEvent" />.</typeparam>
public interface IEventHandler<in TEvent> where TEvent : IEvent
{
  /// <summary>Handles the incoming event.</summary>
  /// <param name="event">The event instance to be processed.</param>
  /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
  /// <returns>A task representing the asynchronous operation.</returns>
  Task Handle(TEvent @event, CancellationToken cancellationToken);
}
