namespace RGamaFelix.CqrsDispatcher.Event.Pipeline.Request;

/// <summary>
///   Defines a request-level pipeline extension that runs once before any event handler is invoked.
///   Used for cross-cutting concerns such as validation or logging applied to all handlers of an event.
/// </summary>
/// <typeparam name="TEvent">The event type. Must implement <see cref="IEvent" />.</typeparam>
public interface IEventRequestExtension<TEvent> where TEvent : IEvent
{
  /// <summary>Gets the execution order. Lower values execute first (outermost in the pipeline).</summary>
  int? Order { get; }

  /// <summary>Executes behavior around the full event dispatch.</summary>
  Task Handle(TEvent @event, Func<TEvent, CancellationToken, Task> next, CancellationToken cancellationToken);

  /// <summary>Determines whether this extension should run for the given event.</summary>
  bool ShouldRun(TEvent @event);
}