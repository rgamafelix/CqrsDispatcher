using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using RGamaFelix.CqrsDispatcher.Event;
using RGamaFelix.CqrsDispatcher.Event.Handler;
using RGamaFelix.CqrsDispatcher.Event.Pipeline.Handler;

namespace RGamaFelix.CqrsDispatcher.Resilience;

/// <summary>
///   Retries a failing event handler up to the number of attempts declared via
///   <see cref="RetryPolicyAttribute" /> on the handler class. If no attribute is present,
///   <see cref="ShouldRun" /> returns <c>false</c> and the extension is a no-op.
/// </summary>
public class RetryEventHandlerExtension<THandler, TEvent>(
  ILogger<IEventHandlerExtension<THandler, TEvent>> logger)
  : IEventHandlerExtension<THandler, TEvent>
  where THandler : IEventHandler<TEvent> where TEvent : IEvent
{
  private static readonly ConcurrentDictionary<Type, RetryPolicyAttribute?> _cache = new();

  public int? Order => 0;

  public bool ShouldRun(TEvent @event)
  {
    return GetAttribute(typeof(THandler)) is not null;
  }

  public async Task Handle(TEvent @event, THandler handler, Func<TEvent, CancellationToken, Task> next,
    CancellationToken cancellationToken)
  {
    var attribute = GetAttribute(typeof(THandler))!;
    List<Exception>? exceptions = null;

    for (var attempt = 0; attempt < attribute.MaxAttempts; attempt++)
    {
      cancellationToken.ThrowIfCancellationRequested();

      try
      {
        await next(@event, cancellationToken);

        return;
      }
      catch (OperationCanceledException)
      {
        throw;
      }
      catch (Exception ex)
      {
        (exceptions ??= []).Add(ex);
        logger.LogWarning(ex, "Handler '{Handler}' failed on attempt {Attempt}/{Max}.", typeof(THandler).Name,
          attempt + 1, attribute.MaxAttempts);

        if (attempt < attribute.MaxAttempts - 1 && attribute.DelayMilliseconds > 0)
        {
          await Task.Delay(attribute.DelayMilliseconds, cancellationToken);
        }
      }
    }

    throw new EventHandlerRetryExhaustedException(typeof(THandler), attribute.MaxAttempts, exceptions!);
  }

  private static RetryPolicyAttribute? GetAttribute(Type handlerType)
  {
    return _cache.GetOrAdd(handlerType,
      static t => t.GetCustomAttributes(typeof(RetryPolicyAttribute), true)
        .Cast<RetryPolicyAttribute>()
        .FirstOrDefault());
  }
}
