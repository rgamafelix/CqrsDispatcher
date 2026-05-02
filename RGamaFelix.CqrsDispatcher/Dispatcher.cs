using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RGamaFelix.CqrsDispatcher.Command;
using RGamaFelix.CqrsDispatcher.Command.Handler;
using RGamaFelix.CqrsDispatcher.Command.Pipeline.Handler;
using RGamaFelix.CqrsDispatcher.Command.Pipeline.Request;
using RGamaFelix.CqrsDispatcher.Event;
using RGamaFelix.CqrsDispatcher.Event.Handler;
using RGamaFelix.CqrsDispatcher.Event.Pipeline.Handler;
using RGamaFelix.CqrsDispatcher.Event.Pipeline.Request;
using RGamaFelix.CqrsDispatcher.Exceptions;
using RGamaFelix.CqrsDispatcher.Query;
using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Query.Handler.Selector;
using RGamaFelix.CqrsDispatcher.Query.Pipeline.Handler;
using RGamaFelix.CqrsDispatcher.Query.Pipeline.Request;

namespace RGamaFelix.CqrsDispatcher;

/// <summary>Provides methods for dispatching command, query, and event requests for processing.</summary>
public class Dispatcher
{
  private static readonly ConcurrentDictionary<Type, Type[]> _interfaceCache = new();
  private static readonly ConcurrentDictionary<Type, PropertyInfo?> _orderPropertyCache = new();
  private static readonly ConcurrentDictionary<Type, MethodInfo?> _shouldRunMethodCache = new();
  private readonly ILogger<Dispatcher>? _logger;
  private readonly IServiceProvider _serviceProvider;

  /// <summary>Provides methods for dispatching command, query, and event requests for processing.</summary>
  public Dispatcher(IServiceProvider serviceProvider, ILogger<Dispatcher>? logger)
  {
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  /// <summary>
  ///   Notifies all registered event handlers of the given event concurrently (fire-and-forget).
  ///   Unlike commands, events may have zero or more handlers; no exception is thrown when no handlers are registered.
  /// </summary>
  /// <param name="event">The event to publish.</param>
  /// <param name="onException">Optional callback invoked for each handler that throws.</param>
  /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
  /// <typeparam name="TEvent">The event type. Must implement <see cref="IEvent" />.</typeparam>
  /// <returns>A task representing the asynchronous notification operation.</returns>
  public Task Notify<TEvent>(TEvent @event, Action<Exception>? onException = null,
    CancellationToken cancellationToken = default) where TEvent : IEvent
  {
    using var loggerScope = _logger?.BeginScope(new Dictionary<string, object>
    {
      ["EventType"] = typeof(TEvent).Name
    });

    if (@event is null)
    {
      _logger?.LogError("Null event");

      throw new ArgumentNullException(nameof(@event));
    }

    cancellationToken.ThrowIfCancellationRequested();

    return Task.Run(async () =>
    {
      try
      {
        cancellationToken.ThrowIfCancellationRequested();
        using var serviceScope = _serviceProvider.CreateScope();
        await InternalNotify(@event, serviceScope.ServiceProvider, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
      }
      catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
      {
        _logger?.LogDebug("Event processing was cancelled for {EventType}", typeof(TEvent).Name);

        throw;
      }
      catch (Exception ex)
      {
        _logger?.LogError(ex, "An error occurred while processing the event");

        if (onException != null)
        {
          onException.Invoke(ex);
        }
        else
        {
          throw;
        }
      }
    }, cancellationToken);
  }

  /// <summary>Publishes a command request for processing by exactly one registered handler.</summary>
  /// <param name="request">The command request to process.</param>
  /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
  /// <typeparam name="TRequest">The command request type. Must implement <see cref="ICommandRequest" />.</typeparam>
  /// <returns>A task representing the asynchronous command processing operation.</returns>
  /// <exception cref="NoHandlerRegisteredException{TRequest}">Thrown when no handler is registered.</exception>
  /// <exception cref="MultipleCommandHandlersRegisteredException{TRequest}">Thrown when more than one handler is registered.</exception>
  public async Task Publish<TRequest>(TRequest request, CancellationToken cancellationToken = default)
    where TRequest : ICommandRequest
  {
    using var loggerScope = _logger?.BeginScope(new Dictionary<string, object>
    {
      ["RequestType"] = typeof(TRequest).Name
    });

    if (request is null)
    {
      _logger?.LogError("Null request");

      throw new ArgumentNullException(nameof(request));
    }

    cancellationToken.ThrowIfCancellationRequested();

    try
    {
      using var serviceScope = _serviceProvider.CreateScope();
      await InternalPublish(request, serviceScope.ServiceProvider, cancellationToken).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      _logger?.LogError(e, "An error occurred while processing the command request");

      throw;
    }
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
      using var loggerScope = _logger?.BeginScope(new Dictionary<string, object>
      {
        ["RequestType"] = typeof(TRequest).Name
      });

      if (request is null)
      {
        _logger?.LogError("Null request");

        throw new ArgumentNullException(nameof(request));
      }

      cancellationToken.ThrowIfCancellationRequested();
      var handler = GetQueryHandler<TRequest, TResponse>(request);
      var handlerExecutionPipeline = GetQueryHandlerExtensions(request, handler);
      var requestExtensions = GetQueryRequestExtensions<TRequest, TResponse>(request);

      var completePipeline = BuildPipeline(handlerExecutionPipeline, requestExtensions, (extension, next) =>
        WithCancellationCheck<TRequest, TResponse>(async (req, ct) =>
        {
          if (extension != null)
          {
            return await ((IQueryRequestExtension<TRequest, TResponse>)extension).Handle(req, next, ct)
              .ConfigureAwait(false);
          }

          throw new NullReferenceException($"Null extension in query pipeline for request {typeof(TRequest).Name}");
        }));

      return await completePipeline(request, cancellationToken).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      _logger?.LogError(e, "An error occurred while processing the query request");

      throw;
    }
  }

  private static TDelegate BuildPipeline<TDelegate>(TDelegate coreHandler, IEnumerable<object?> extensions,
    Func<object?, TDelegate, TDelegate> wrapExtension)
  {
    var pipeline = coreHandler;

    foreach (var extension in extensions)
    {
      pipeline = wrapExtension(extension, pipeline);
    }

    return pipeline;
  }

  private static ICommandHandler<TRequest> GetCommandHandler<TRequest>(IServiceProvider provider)
    where TRequest : ICommandRequest
  {
    var requestType = typeof(TRequest);
    var handlerType = typeof(ICommandHandler<>).MakeGenericType(requestType);

    var handlers = provider.GetServices(handlerType)
      .Where(handler => IsHandlerCompatible(handler, requestType))
      .Cast<ICommandHandler<TRequest>>()
      .ToList();

    return handlers.Count switch
    {
      0 => throw new NoHandlerRegisteredException<TRequest>(),
      > 1 => throw new MultipleCommandHandlersRegisteredException<TRequest>(),
      _ => handlers[0]
    };
  }

  private static List<IEventHandler<TEvent>> GetEventHandlers<TEvent>(IServiceProvider provider) where TEvent : IEvent
  {
    var eventType = typeof(TEvent);
    var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);

    return provider.GetServices(handlerType)
      .Where(handler => IsEventHandlerCompatible(handler, eventType))
      .Cast<IEventHandler<TEvent>>()
      .ToList();
  }

  private static int GetExecutionOrder(object? extension)
  {
    if (extension is null)
    {
      return 0;
    }

    var extensionType = extension.GetType();
    var orderProperty = _orderPropertyCache.GetOrAdd(extensionType, type => type.GetProperty("Order"));

    if (orderProperty is null)
    {
      return 0;
    }

    return (int?)orderProperty.GetValue(extension, null) ?? 0;
  }

  private static bool IsCommandExtensionCompatible(object? extension, Type requestType)
  {
    if (extension is null)
    {
      return false;
    }

    var extensionTypeInfo = extension.GetType();
    var interfaces = _interfaceCache.GetOrAdd(extensionTypeInfo, type => type.GetInterfaces());

    var interfaceType = interfaces.FirstOrDefault(i =>
      i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandRequestExtension<>));

    var handledType = interfaceType?.GetGenericArguments()[0];

    return handledType != null && handledType.IsAssignableFrom(requestType);
  }

  private static bool IsEventHandlerCompatible(object? handler, Type eventType)
  {
    if (handler is null)
    {
      return false;
    }

    var handlerType = handler.GetType();
    var interfaces = _interfaceCache.GetOrAdd(handlerType, type => type.GetInterfaces());

    var handlerInterface =
      interfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>));

    var handledType = handlerInterface?.GetGenericArguments()[0];

    return handledType != null && handledType.IsAssignableFrom(eventType);
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

  private static bool SatisfyCondition(object? extension, object request)
  {
    if (extension is null)
    {
      return false;
    }

    var extensionType = extension.GetType();
    var shouldRunMethod = _shouldRunMethodCache.GetOrAdd(extensionType, type => type.GetMethod("ShouldRun"));

    if (shouldRunMethod is null)
    {
      return true;
    }

    var result = shouldRunMethod.Invoke(extension, [request]);

    return result is true;
  }

  private static Func<TRequest, CancellationToken, Task<TResponse>> WithCancellationCheck<TRequest, TResponse>(
    Func<TRequest, CancellationToken, Task<TResponse>> next)
  {
    return async (request, cancellationToken) =>
    {
      cancellationToken.ThrowIfCancellationRequested();
      var response = await next(request, cancellationToken).ConfigureAwait(false);
      cancellationToken.ThrowIfCancellationRequested();

      return response;
    };
  }

  private static Func<TRequest, CancellationToken, Task> WithCancellationCheck<TRequest>(
    Func<TRequest, CancellationToken, Task> next)
  {
    return async (request, cancellationToken) =>
    {
      cancellationToken.ThrowIfCancellationRequested();
      await next(request, cancellationToken).ConfigureAwait(false);
      cancellationToken.ThrowIfCancellationRequested();
    };
  }

  private async Task AllEventHandlers<TEvent>(TEvent evt, List<IEventHandler<TEvent>> handlers, TEvent originalEvent,
    IServiceProvider provider, CancellationToken token) where TEvent : IEvent
  {
    token.ThrowIfCancellationRequested();

    var tasks = handlers.Select(async handler =>
    {
      token.ThrowIfCancellationRequested();
      var handlerRuntimeType = handler.GetType();
      var handlerServiceType = typeof(IEventHandler<TEvent>);

      var runtimeTypePipelines = GetExtensions<dynamic>(
        typeof(IEventHandlerExtension<,>).MakeGenericType(handlerRuntimeType, typeof(TEvent)), typeof(TEvent),
        originalEvent, "event handler pipelines", provider);

      var serviceTypePipelines = GetExtensions<dynamic>(
        typeof(IEventHandlerExtension<,>).MakeGenericType(handlerServiceType, typeof(TEvent)), typeof(TEvent),
        originalEvent, "event handler pipelines", provider);

      var handlerPipelines = runtimeTypePipelines.Concat(serviceTypePipelines).ToList();
      var handlerDelegate = WithCancellationCheck<TEvent>(handler.Handle);

      var pipeline = BuildPipeline(handlerDelegate, handlerPipelines, (extension, next) =>
        WithCancellationCheck<TEvent>(async (e, ct) =>
        {
          if (extension != null)
          {
            await ((Task)((dynamic)extension).Handle((dynamic)e, (dynamic)handler, next, ct)).ConfigureAwait(false);

            return;
          }

          throw new NullReferenceException($"Null extension in event pipeline for event {typeof(TEvent).Name}");
        }));

      await pipeline(evt, token).ConfigureAwait(false);
    });

    await Task.WhenAll(tasks).ConfigureAwait(false);
    token.ThrowIfCancellationRequested();
  }

  private List<T> GetExtensions<T>(Type extensionGenericType, Type requestType, object request, string logMessage,
    IServiceProvider provider)
  {
    var extensions = provider.GetServices(extensionGenericType);
    var aux1 = extensions.Where(p => SatisfyCondition(p, request));
    var aux2 = aux1.OrderByDescending(GetExecutionOrder);
    var aux3 = aux2.Cast<T>();
    var aux4 = aux3.ToList();
    _logger?.LogDebug("{amount} {logMessage} found for {requestType}", aux4.Count, logMessage, requestType);

    return aux4;
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

  private Func<TRequest, CancellationToken, Task<TResponse>> GetQueryHandlerExtensions<TRequest, TResponse>(
    TRequest request, IQueryHandler<TRequest, TResponse> handler) where TRequest : IQueryRequest<TResponse>
  {
    var handlerRuntimeType = handler.GetType();
    var handlerServiceType = typeof(IQueryHandler<TRequest, TResponse>);

    var runtimeTypePipelines = GetExtensions<dynamic>(
      typeof(IQueryHandlerExtension<,,>).MakeGenericType(handlerRuntimeType, typeof(TRequest), typeof(TResponse)),
      typeof(TRequest), request, "query handler pipelines", _serviceProvider);

    var serviceTypePipelines = GetExtensions<dynamic>(
      typeof(IQueryHandlerExtension<,,>).MakeGenericType(handlerServiceType, typeof(TRequest), typeof(TResponse)),
      typeof(TRequest), request, "query handler pipelines", _serviceProvider);

    var handlerPipelines = runtimeTypePipelines.Concat(serviceTypePipelines).ToList();
    var handlerDelegate = WithCancellationCheck<TRequest, TResponse>(handler.Handle);

    return BuildPipeline(handlerDelegate, handlerPipelines, (extension, next) =>
      WithCancellationCheck<TRequest, TResponse>(async (r, ct) =>
      {
        if (extension != null)
        {
          return await ((Task<TResponse>)((dynamic)extension).Handle((dynamic)r, (dynamic)handler, next, ct))
            .ConfigureAwait(false);
        }

        throw new NullReferenceException($"Null extension in query pipeline for request {typeof(TRequest).Name}");
      }));
  }

  private List<IQueryRequestExtension<TRequest, TResponse>> GetQueryRequestExtensions<TRequest, TResponse>(
    TRequest request) where TRequest : IQueryRequest<TResponse>
  {
    return GetExtensions<IQueryRequestExtension<TRequest, TResponse>>(
      typeof(IQueryRequestExtension<,>).MakeGenericType(typeof(TRequest), typeof(TResponse)), typeof(TRequest), request,
      "query request pipelines", _serviceProvider);
  }

  private List<object?> GetRequestCommandExtensions<TRequest>(TRequest request, IServiceProvider provider)
    where TRequest : ICommandRequest
  {
    var requestType = typeof(TRequest);
    var extensionType = typeof(ICommandRequestExtension<>).MakeGenericType(requestType);

    var requestPipelines = provider.GetServices(extensionType)
      .Where(extension => IsCommandExtensionCompatible(extension, requestType))
      .Where(p => SatisfyCondition(p, request))
      .OrderByDescending(GetExecutionOrder)
      .ToList();

    _logger?.LogDebug("{amount} command request pipelines found for {requestType}", requestPipelines.Count,
      requestType);

    return requestPipelines;
  }

  private List<object?> GetRequestEventExtensions<TEvent>(TEvent @event, IServiceProvider provider)
    where TEvent : IEvent
  {
    var eventType = typeof(TEvent);
    var extensionType = typeof(IEventRequestExtension<>).MakeGenericType(eventType);

    var requestPipelines = provider.GetServices(extensionType)
      .Where(p => SatisfyCondition(p, @event))
      .OrderByDescending(GetExecutionOrder)
      .ToList();

    _logger?.LogDebug("{amount} event request pipelines found for {eventType}", requestPipelines.Count, eventType);

    return requestPipelines;
  }

  private async Task InternalNotify<TEvent>(TEvent @event, IServiceProvider provider,
    CancellationToken cancellationToken) where TEvent : IEvent
  {
    cancellationToken.ThrowIfCancellationRequested();
    var handlers = GetEventHandlers<TEvent>(provider);

    if (handlers.Count == 0)
    {
      _logger?.LogDebug("No event handlers registered for {EventType}", typeof(TEvent).Name);

      return;
    }

    var requestPipelines = GetRequestEventExtensions(@event, provider);

    var fullPipeline = BuildPipeline(
      WithCancellationCheck<TEvent>((evt, token) => AllEventHandlers(evt, handlers, @event, provider, token)),
      requestPipelines,
      (extension, next) => WithCancellationCheck<TEvent>((e, ct) =>
        ((IEventRequestExtension<TEvent>)extension!).Handle(e, next, ct)));

    await fullPipeline(@event, cancellationToken).ConfigureAwait(false);
    cancellationToken.ThrowIfCancellationRequested();
  }

  private async Task InternalPublish<TRequest>(TRequest request, IServiceProvider provider,
    CancellationToken cancellationToken) where TRequest : ICommandRequest
  {
    cancellationToken.ThrowIfCancellationRequested();
    var handler = GetCommandHandler<TRequest>(provider);
    var requestPipelines = GetRequestCommandExtensions(request, provider);
    var handlerType = handler.GetType();

    var handlerPipelines = GetExtensions<dynamic>(
      typeof(ICommandHandlerExtension<,>).MakeGenericType(handlerType, typeof(TRequest)), typeof(TRequest), request,
      "command handler pipelines", provider);

    var handlerDelegate = WithCancellationCheck<TRequest>(handler.Handle);

    var handlerWithExtensions = BuildPipeline(handlerDelegate, handlerPipelines, (extension, next) =>
      WithCancellationCheck<TRequest>(async (r, ct) =>
      {
        if (extension != null)
        {
          await ((Task)((dynamic)extension).Handle((dynamic)r, (dynamic)handler, next, ct)).ConfigureAwait(false);

          return;
        }

        throw new NullReferenceException($"Null extension in command pipeline for request {typeof(TRequest).Name}");
      }));

    var fullPipeline = BuildPipeline(handlerWithExtensions, requestPipelines,
      (extension, next) => WithCancellationCheck<TRequest>((r, ct) =>
        ((ICommandRequestExtension<TRequest>)extension!).Handle(r, next, ct)));

    await fullPipeline(request, cancellationToken).ConfigureAwait(false);
    cancellationToken.ThrowIfCancellationRequested();
  }
}
