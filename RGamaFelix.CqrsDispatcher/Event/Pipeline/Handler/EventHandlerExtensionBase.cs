using RGamaFelix.CqrsDispatcher.Event.Handler;

namespace RGamaFelix.CqrsDispatcher.Event.Pipeline.Handler;

/// <summary>
///   Base class for event handler extensions. Provides <c>Order = 0</c> and <c>ShouldRun = true</c> defaults.
/// </summary>
public abstract class EventHandlerExtensionBase<THandler, TEvent> : IEventHandlerExtension<THandler, TEvent>
  where THandler : IEventHandler<TEvent> where TEvent : IEvent
{
  /// <inheritdoc />
  public virtual int? Order { get; } = 0;

  /// <inheritdoc />
  public abstract Task Handle(TEvent @event, THandler handler, Func<TEvent, CancellationToken, Task> next,
    CancellationToken cancellationToken);

  /// <inheritdoc />
  public virtual bool ShouldRun(TEvent @event) => true;
}
