using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RGamaFelix.CqrsDispatcher.Command;
using RGamaFelix.CqrsDispatcher.Command.Extension.Handler;
using RGamaFelix.CqrsDispatcher.Command.Extension.Request;
using RGamaFelix.CqrsDispatcher.Command.Handler;
using RGamaFelix.CqrsDispatcher.Exceptions;
using RGamaFelix.CqrsDispatcher.Query;
using RGamaFelix.CqrsDispatcher.Query.Extension.Handler;
using RGamaFelix.CqrsDispatcher.Query.Extension.Request;
using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Query.Handler.Selector;

namespace RGamaFelix.CqrsDispatcher;

/// <summary>Provides methods for dispatching command and query requests for processing.</summary>
public class Dispatcher
{
  private static readonly ConcurrentDictionary<Type, Type[]> _interfaceCache = new();
  private static readonly ConcurrentDictionary<Type, PropertyInfo?> _orderPropertyCache = new();
  private static readonly ConcurrentDictionary<Type, MethodInfo?> _shouldRunMethodCache = new();
  private readonly ILogger<Dispatcher>? _logger;
  private readonly IServiceProvider _serviceProvider;

  /// <summary>Provides methods for dispatching command and query requests for processing.</summary>
  public Dispatcher(IServiceProvider serviceProvider, ILogger<Dispatcher>? logger)
  {
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  /// <summary>Publishes a command request for asynchronous processing.</summary>
  /// <param name="request">The command request to be processed, implementing <see cref="ICommandRequest" />.</param>
  /// <param name="onException">Optional callback to handle exceptions that may occur during processing.</param>
  /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
  /// <typeparam name="TRequest">The type of the command request, constrained to <see cref="ICommandRequest" />.</typeparam>
  /// <returns>A task representing the asynchronous command processing operation.</returns>
  public Task Publish<TRequest>(TRequest request, Action<Exception>? onException = null,
    CancellationToken cancellationToken = default) where TRequest : ICommandRequest
  {
    return Task.Run(async () =>
    {
      try
      {
        using var loggerScope = _logger?.BeginScope(new Dictionary<string, object>
        {
          ["RequestType"] = typeof(TRequest).Name
        });

        using var serviceScope = _serviceProvider.CreateScope();
        var scopedProvider = serviceScope.ServiceProvider;
        await InternalPublish(request, scopedProvider, cancellationToken).ConfigureAwait(false);
      }
      catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
      {
        _logger?.LogDebug("Command processing was cancelled for {RequestType}", typeof(TRequest).Name);

        throw;
      }
      catch (Exception ex)
      {
        _logger?.LogError(ex, "An error occurred while processing the command request");
        onException?.Invoke(ex);

        throw;
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
    CancellationToken cancellationToken = default) where TRequest : IQueryRequest<TResponse>
  {
    try
    {
      ArgumentNullException.ThrowIfNull(request);
      using var loggerScope = _logger?.BeginScope(typeof(TRequest).Name);
      var handler = GetQueryHandler<TRequest, TResponse>(request);
      var handlerExecutionPipeline = GetQueryHandlerBehaviors(request, handler);
      var requestBehaviors = GetQueryRequestBehaviors<TRequest, TResponse>(request);

      var completePipeline = BuildPipeline(handlerExecutionPipeline, requestBehaviors,
        (behavior, next) => (req, ct) =>
          behavior != null
            ? ((IQueryRequestExtension<TRequest, TResponse>)behavior).Handle(req, next, ct)
            : throw new NullReferenceException($"Null behavior in query pipeline for request {typeof(TRequest).Name}"));

      return await completePipeline(request, cancellationToken);
    }
    catch (Exception e)
    {
      _logger?.LogError(e, "An error occurred while processing the query request");

      throw;
    }
  }

  private static TDelegate BuildPipeline<TDelegate>(TDelegate coreHandler, IEnumerable<object?> behaviors,
    Func<object?, TDelegate, TDelegate> wrapBehavior)
  {
    var pipeline = coreHandler;

    foreach (var behavior in behaviors)
    {
      pipeline = wrapBehavior(behavior, pipeline);
    }

    return pipeline;
  }

  private static List<object> GetCommandHandlers<TRequest>(IServiceProvider provider) where TRequest : ICommandRequest
  {
    var requestType = typeof(TRequest);
    var handlerType = typeof(ICommandHandler<>).MakeGenericType(requestType);

    var handlers = provider.GetServices(handlerType)
      .Where(handler => IsHandlerCompatible(handler, requestType))
      .Cast<object>()
      .ToList();

    return handlers.Count == 0 ? throw new NoHandlerRegisteredException<TRequest>() : handlers;
  }

  private static int GetExecutionOrder(object? behavior)
  {
    if (behavior is null)
    {
      return 0;
    }

    var behaviorType = behavior.GetType();
    var orderProperty = _orderPropertyCache.GetOrAdd(behaviorType, type => type.GetProperty("Order"));

    if (orderProperty is null)
    {
      return 0;
    }

    return (int?)orderProperty.GetValue(behavior, null) ?? 0;
  }

  private static bool IsCommandBehaviorCompatible(object? behavior, Type requestType)
  {
    if (behavior is null)
    {
      return false;
    }

    var behaviorTypeInfo = behavior.GetType();
    var interfaces = _interfaceCache.GetOrAdd(behaviorTypeInfo, type => type.GetInterfaces());

    var interfaceType = interfaces.FirstOrDefault(i =>
      i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandRequestExtension<>));

    var handledType = interfaceType?.GetGenericArguments()[0];

    return handledType != null && handledType.IsAssignableFrom(requestType);
  }

  private static bool IsHandlerCompatible(object? handler, Type requestType)
  {
    if (handler is null)
    {
      return false;
    }

    var handlerType = handler.GetType();
    var interfaces = _interfaceCache.GetOrAdd(handlerType, type => type.GetInterfaces());

    var handlerInterface =
      interfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<>));

    var handledType = handlerInterface?.GetGenericArguments()[0];

    return handledType != null && handledType.IsAssignableFrom(requestType);
  }

  private static bool SatisfyCondition(object? behavior, object request)
  {
    if (behavior is null)
    {
      return false;
    }

    var behaviorType = behavior.GetType();
    var shouldRunMethod = _shouldRunMethodCache.GetOrAdd(behaviorType, type => type.GetMethod("ShouldRun"));

    if (shouldRunMethod is null)
    {
      return true;
    }

    var result = shouldRunMethod.Invoke(behavior, [request]);

    return result is true;
  }

  private async Task AllHandlers<TRequest>(TRequest req, List<object> handlers, TRequest originalRequest,
    IServiceProvider provider, CancellationToken token) where TRequest : ICommandRequest
  {
    var tasks = handlers.Select(async handler =>
    {
      var handlerType = handler.GetType();

      var handlerPipelines = GetBehaviors<dynamic>(
        typeof(ICommandHandlerExtension<,>).MakeGenericType(handlerType, typeof(TRequest)), typeof(TRequest),
        originalRequest, "command handler pipelines", provider);

      var handlerDelegate = (handler as ICommandHandler<TRequest>)!.Handle;

      var pipeline = BuildPipeline(handlerDelegate, handlerPipelines,
        (behavior, next) => (r, ct) =>
          behavior != null
            ? (Task)((dynamic)behavior).Handle((dynamic)r, (dynamic)handler, next, ct)
            : throw new NullReferenceException(
              $"Null behavior in command pipeline for request {typeof(TRequest).Name}"));

      await pipeline(req, token);
    });

    await Task.WhenAll(tasks);
  }

  private List<T> GetBehaviors<T>(Type behaviorGenericType, Type requestType, object request, string logMessage,
    IServiceProvider provider)
  {
    var behaviors = provider.GetServices(behaviorGenericType)
      .Where(p => SatisfyCondition(p, request))
      .OrderBy(GetExecutionOrder)
      .Cast<T>()
      .ToList();

    _logger?.LogDebug("{amount} {logMessage} found for {requestType}", behaviors.Count, logMessage, requestType);

    return behaviors;
  }

  private IQueryHandler<TRequest, TResponse> GetQueryHandler<TRequest, TResponse>(TRequest request)
    where TRequest : IQueryRequest<TResponse>
  {
    var handlers = _serviceProvider.GetServices<IQueryHandler<TRequest, TResponse>>().ToList();

    switch (handlers.Count)
    {
      case 0:
        throw new NoHandlerRegisteredException<TRequest>();
      case 1:
        return handlers.First();
    }

    var selectorList = _serviceProvider.GetServices<IQueryHandlerSelector<TRequest, TResponse>>().ToList();

    switch (selectorList.Count)
    {
      case 0:
        throw new NoHandlerSelectorRegisteredException<TRequest>();
      case > 1:
        throw new MultipleSelectorsRegisteredException<TRequest>();
      default:
      {
        var handler = selectorList.First().SelectHandler(request, handlers);

        return handler ?? throw new MultipleQueryHandlersRegisteredException<TRequest>();
      }
    }
  }

  private Func<TRequest, CancellationToken, Task<TResponse>> GetQueryHandlerBehaviors<TRequest, TResponse>(
    TRequest request, IQueryHandler<TRequest, TResponse> handler) where TRequest : IQueryRequest<TResponse>
  {
    var handlerType = handler.GetType();

    var handlerPipelines = GetBehaviors<dynamic>(
      typeof(IQueryHandlerExtension<,,>).MakeGenericType(handlerType, typeof(TRequest), typeof(TResponse)),
      typeof(TRequest), request, "query handler pipelines", _serviceProvider);

    Func<TRequest, CancellationToken, Task<TResponse>> handlerDelegate = handler.HandleAsync;

    return BuildPipeline(handlerDelegate, handlerPipelines,
      (behavior, next) => (r, ct) =>
        behavior != null
          ? (Task<TResponse>)((dynamic)behavior).Handle((dynamic)r, (dynamic)handler, next, ct)
          : throw new NullReferenceException($"Null behavior in query pipeline for request {typeof(TRequest).Name}"));
  }

  private List<IQueryRequestExtension<TRequest, TResponse>> GetQueryRequestBehaviors<TRequest, TResponse>(
    TRequest request) where TRequest : IQueryRequest<TResponse>
  {
    return GetBehaviors<IQueryRequestExtension<TRequest, TResponse>>(
      typeof(IQueryRequestExtension<,>).MakeGenericType(typeof(TRequest), typeof(TResponse)), typeof(TRequest), request,
      "query request pipelines", _serviceProvider);
  }

  private List<object?> GetRequestCommandBehaviors<TRequest>(TRequest request, IServiceProvider provider)
    where TRequest : ICommandRequest
  {
    var requestType = typeof(TRequest);
    var behaviorType = typeof(ICommandRequestExtension<>).MakeGenericType(requestType);

    var requestPipelines = provider.GetServices(behaviorType)
      .Where(behavior => IsCommandBehaviorCompatible(behavior, requestType))
      .Where(p => SatisfyCondition(p, request))
      .OrderBy(GetExecutionOrder)
      .ToList();

    _logger?.LogDebug("{amount} command request pipelines found for {requestType}", requestPipelines.Count,
      requestType);

    return requestPipelines;
  }

  private async Task InternalPublish<TRequest>(TRequest request, IServiceProvider provider,
    CancellationToken cancellationToken) where TRequest : ICommandRequest
  {
    var handlers = GetCommandHandlers<TRequest>(provider);
    var requestPipelines = GetRequestCommandBehaviors(request, provider);

    var fullPipeline = BuildPipeline(
      (Func<TRequest, CancellationToken, Task>)((req, token) => AllHandlers(req, handlers, request, provider, token)),
      requestPipelines,
      (behavior, next) => (r, ct) => ((ICommandRequestExtension<TRequest>)behavior!).Handle(r, next, ct));

    await fullPipeline(request, cancellationToken);
  }
}
