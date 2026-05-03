namespace RGamaFelix.CqrsDispatcher.Resilience;

/// <summary>
///   Thrown when all retry attempts for an event handler have been exhausted.
///   Inner exceptions contain the failure from each individual attempt.
/// </summary>
public class EventHandlerRetryExhaustedException : AggregateException
{
  public EventHandlerRetryExhaustedException(Type handlerType, int maxAttempts, IEnumerable<Exception> innerExceptions)
    : base($"Handler '{handlerType.Name}' failed after {maxAttempts} attempt(s).", innerExceptions)
  {
    HandlerType = handlerType;
    MaxAttempts = maxAttempts;
  }

  public Type HandlerType { get; }
  public int MaxAttempts { get; }
}
