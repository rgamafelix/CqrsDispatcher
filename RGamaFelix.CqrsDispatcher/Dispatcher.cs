using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RGamaFelix.CqrsDispatcher.Command;
using RGamaFelix.CqrsDispatcher.Command.Handler;
using RGamaFelix.CqrsDispatcher.Command.Pipeline.Handler;
using RGamaFelix.CqrsDispatcher.Command.Pipeline.Request;
using RGamaFelix.CqrsDispatcher.Exceptions;
using RGamaFelix.CqrsDispatcher.Query;
using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Query.Handler.Selector;
using RGamaFelix.CqrsDispatcher.Query.Pipeline.Handler;
using RGamaFelix.CqrsDispatcher.Query.Pipeline.Request;

namespace RGamaFelix.CqrsDispatcher;

/// <summary>Provides methods for dispatching command and query requests for processing.</summary>
public class Dispatcher
{
  private readonly ILogger<Dispatcher> _logger;
  private readonly IServiceProvider _serviceProvider;

  /// <summary>Provides methods for dispatching command and query requests for processing.</summary>
  public Dispatcher(IServiceProvider serviceProvider, ILogger<Dispatcher> logger)
  {
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  /// <summary>Publishes a command request for asynchronous processing.</summary>
  /// <param name="request">The command request to be processed, implementing <see cref="ICommandRequest" />.</param>
  /// <param name="onException">Optional callback to handle exceptions that may occur during processing.</param>
  /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
  /// <typeparam name="TRequest">The type of the command request, constrained to <see cref="ICommandRequest" />.</typeparam>
  public void Publish<TRequest>(TRequest request, Action<Exception>? onException = null,
    CancellationToken cancellationToken = default)
    where TRequest : ICommandRequest
  {
    using var scope = _logger.BeginScope(typeof(TRequest).Name);

    _ = Task.Run(async () =>
    {
      try
      {
        await InternalPublish(request, cancellationToken)
          .ConfigureAwait(false);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "An error occurred while processing the command request");
        onException?.Invoke(ex);
      }
    }, cancellationToken);
  }

  /// <summary>Sends a query request for processing and retrieves the corresponding response.</summary>
  /// <typeparam name="TRequest">The type of the query request being processed.</typeparam>
  /// <typeparam name="TResponse">The type of the response expected from the query request.</typeparam>
  /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
  /// <param name="request">The query request to be processed.</param>
  /// <returns>A task representing the asynchronous operation, with the result being the response of the query request.</returns>
  /// <exception cref="Exception">Thrown when no handler or handler selector is registered for the provided request type.</exception>
  public async Task<TResponse> Send<TRequest, TResponse>(TRequest request,
    CancellationToken cancellationToken = default)
    where TRequest : IQueryRequest<TResponse>
  {
    using var scope = _logger.BeginScope(typeof(TRequest).Name);
    var handler = GetQueryHandler<TRequest, TResponse>(request);
    var handlerDelegate = GetQueryHandlerBehaviors(request, handler);
    var requestPipelines = GetQueryRequestBehaviors<TRequest, TResponse>(request);
    var fullPipeline = handlerDelegate;

    foreach (var pipeline in requestPipelines)
    {
      var next = fullPipeline;
      fullPipeline = (r, ct) => pipeline.Handle(r, next, ct);
    }

    return await fullPipeline(request, cancellationToken);
  }

  private async Task AllHandlers<TRequest>(TRequest req, List<object> handlers, TRequest originalRequest,
    CancellationToken token)
    where TRequest : ICommandRequest
  {
    var tasks = handlers.Select(async handler =>
    {
      var handlerType = handler.GetType();

      var handlerPipelines = _serviceProvider
        .GetServices(typeof(ICommandHandlerBehavior<,>).MakeGenericType(handlerType, typeof(TRequest)))
        .Where(p => SatisfyCondition(p, originalRequest))
        .OrderBy(GetExecutionOrder)
        .Cast<dynamic>()
        .ToList();

      var handlerDelegate = (handler as ICommandHandler<TRequest>)!.Handle;

      foreach (var behavior in handlerPipelines)
      {
        var next = handlerDelegate;
        handlerDelegate = (r, ct) => behavior.Handle((dynamic)r, (dynamic)handler, next, ct);
      }

      await handlerDelegate(req, token);
    });

    await Task.WhenAll(tasks);
  }

  private List<object> GetCommandHandlers<TRequest>()
    where TRequest : ICommandRequest
  {
    var handlers = _serviceProvider.GetServices(typeof(ICommandHandler<>).MakeGenericType(typeof(TRequest)))
      .Where(handler =>
      {
        var handlerInterface = handler?.GetType()
          .GetInterfaces()
          .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<>));

        var handledType = handlerInterface?.GetGenericArguments()[0];

        return handledType != null && handledType.IsAssignableFrom(typeof(TRequest));
      })
      .Cast<object>()
      .ToList();

    return handlers.Count == 0 ? throw new NoHandlerRegisteredException<TRequest>() : handlers;
  }

  private int GetExecutionOrder(object? behavior)
  {
    var orderProperty = behavior?.GetType()
      .GetProperty("Order");

    if (orderProperty is null)
    {
      return 0;
    }

    return (int?)orderProperty.GetValue(behavior, null) ?? 0;
  }

  private IQueryHandler<TRequest, TResponse> GetQueryHandler<TRequest, TResponse>(TRequest request)
    where TRequest : IQueryRequest<TResponse>
  {
    var handlers = _serviceProvider.GetServices<IQueryHandler<TRequest, TResponse>>()
      .ToList();

    if (handlers.Count == 0)
    {
      throw new NoHandlerRegisteredException<TRequest>();
    }

    var selector = _serviceProvider.GetRequiredService<IQueryHandlerSelector<TRequest, TResponse>>();
    var handler = selector.SelectHandler(request, handlers);

    return handler ?? throw new NoHandlerSelectorFoundException<TRequest>();
  }

  private Func<TRequest, CancellationToken, Task<TResponse>> GetQueryHandlerBehaviors<TRequest, TResponse>(
    TRequest request, IQueryHandler<TRequest, TResponse> handler)
    where TRequest : IQueryRequest<TResponse>
  {
    var handlerType = handler.GetType();

    var handlerPipelines = _serviceProvider
      .GetServices(typeof(IQueryHandlerBehavior<,,>).MakeGenericType(handlerType, typeof(TRequest), typeof(TResponse)))
      .Where(p => SatisfyCondition(p, request))
      .OrderBy(GetExecutionOrder)
      .Cast<dynamic>()
      .ToList();

    _logger.LogDebug("{amount} query handler pipelines found for {requestType}", handlerPipelines.Count,
      typeof(TRequest));

    Func<TRequest, CancellationToken, Task<TResponse>> handlerDelegate = handler.HandleAsync;

    foreach (var behavior in handlerPipelines)
    {
      var next = handlerDelegate;
      handlerDelegate = (r, ct) => behavior.Handle((dynamic)r, (dynamic)handler, next, ct);
    }

    return handlerDelegate;
  }

  private List<IQueryRequestBehavior<TRequest, TResponse>> GetQueryRequestBehaviors<TRequest, TResponse>(
    TRequest request)
    where TRequest : IQueryRequest<TResponse>
  {
    var requestPipelines = _serviceProvider.GetServices<IQueryRequestBehavior<TRequest, TResponse>>()
      .Where(p => SatisfyCondition(p, request))
      .OrderBy(GetExecutionOrder)
      .ToList();

    _logger.LogDebug("{amount} query request pipelines found for {requestType}", requestPipelines.Count,
      typeof(TRequest));

    return requestPipelines;
  }

  private List<object?> GetRequestCommandBehaviors<TRequest>(TRequest request)
    where TRequest : ICommandRequest
  {
    var requestPipelines = _serviceProvider
      .GetServices(typeof(ICommandRequestBehavior<>).MakeGenericType(typeof(TRequest)))
      .Where(behavior =>
      {
        var interfaceType = behavior?.GetType()
          .GetInterfaces()
          .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandRequestBehavior<>));

        var handledType = interfaceType?.GetGenericArguments()[0];

        return handledType != null && handledType.IsAssignableFrom(typeof(TRequest));
      })
      .Where(p => SatisfyCondition(p, request))
      .OrderBy(GetExecutionOrder)
      .ToList();

    _logger.LogDebug("{amount} command request pipelines found for {requestType}", requestPipelines.Count,
      typeof(TRequest));

    return requestPipelines;
  }

  private async Task InternalPublish<TRequest>(TRequest request, CancellationToken cancellationToken)
    where TRequest : ICommandRequest
  {
    var handlers = GetCommandHandlers<TRequest>();
    var requestPipelines = GetRequestCommandBehaviors(request);

    var fullPipeline =
      (Func<TRequest, CancellationToken, Task>)((req, token) => AllHandlers(req, handlers, request, token));

    foreach (var pipeline in requestPipelines)
    {
      var next = fullPipeline;
      fullPipeline = (r, ct) => (pipeline as ICommandRequestBehavior<TRequest>)!.Handle(r, next, ct);
    }

    await fullPipeline(request, cancellationToken);
  }

  private bool SatisfyCondition(object? p, object request)
  {
    if (p is null)
    {
      return false;
    }

    var shouldRunMethod = p.GetType()
      .GetMethod("ShouldRun");

    if (shouldRunMethod is null)
    {
      return true;
    }

    var result = shouldRunMethod.Invoke(p, [request]);

    return result is true;
  }
}
