using RGamaFelix.CqrsDispatcher.Query.Handler;

namespace RGamaFelix.CqrsDispatcher.Query.Pipeline.Handler;

/// <summary>
///   Represents a base implementation of a query handler extension, enabling additional
///   behaviors or modifications to be applied during query handling.
/// </summary>
/// <typeparam name="THandler">The type of the query handler implementing IQueryHandler.</typeparam>
/// <typeparam name="TRequest">The type of the query request implementing IQueryRequest.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the query handler.</typeparam>
public abstract class
  QueryHandlerExtensionBase<THandler, TRequest, TResponse> : IQueryHandlerExtension<THandler, TRequest, TResponse>
  where THandler : IQueryHandler<TRequest, TResponse> where TRequest : IQueryRequest<TResponse>
{
  /// <inheritdoc />
  public virtual int? Order { get; } = 0;

  /// <inheritdoc />
  public abstract Task<TResponse> Handle(TRequest request, THandler handler,
    Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken);

  /// <inheritdoc />
  public virtual bool ShouldRun(TRequest request)
  {
    return true;
  }
}