using RGamaFelix.CqrsDispatcher.Event.Handler;

namespace RGamaFelix.CqrsDispatcher.Event.Pipeline.Handler;

/// <summary>Represents a behavior pipeline that wraps individual event handlers.</summary>
/// <typeparam name="THandler">The event handler type. Must implement <see cref="IEventHandler{TEvent}" />.</typeparam>
/// <typeparam name="TEvent">The event type. Must implement <see cref="IEvent" />.</typeparam>
public interface IEventHandlerExtension<THandler, TEvent> where THandler : IEventHandler<TEvent> where TEvent : IEvent
{
  /// <summary>Gets the execution order. Lower values execute first (outermost in the pipeline).</summary>
  int? Order { get; }

  /// <summary>Executes behavior around the event handler.</summary>
  Task Handle(TEvent @event, THandler handler, Func<TEvent, CancellationToken, Task> next,
    CancellationToken cancellationToken);

  /// <summary>Determines whether this extension should run for the given event.</summary>
  bool ShouldRun(TEvent @event);
}
