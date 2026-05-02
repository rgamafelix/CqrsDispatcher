namespace RGamaFelix.CqrsDispatcher.Event.Pipeline.Request;

/// <summary>
///   Base class for event request extensions. Provides <c>Order = 0</c> and <c>ShouldRun = true</c> defaults.
/// </summary>
public abstract class EventRequestExtensionBase<TEvent> : IEventRequestExtension<TEvent> where TEvent : IEvent
{
  /// <inheritdoc />
  public virtual int? Order { get; } = 0;

  /// <inheritdoc />
  public abstract Task Handle(TEvent @event, Func<TEvent, CancellationToken, Task> next,
    CancellationToken cancellationToken);

  /// <inheritdoc />
  public virtual bool ShouldRun(TEvent @event)
  {
    return true;
  }
}