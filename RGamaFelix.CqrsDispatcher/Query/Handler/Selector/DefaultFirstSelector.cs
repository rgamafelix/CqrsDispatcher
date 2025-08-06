namespace RGamaFelix.CqrsDispatcher.Query.Handler.Selector;

/// <summary>
///   Represents a default implementation of <see cref="IQueryHandlerSelector{TRequest, TResponse}" /> that selects
///   the first available handler, prioritizing handlers implementing <see cref="IDefaultHandler{TRequest, TResponse}" />.
/// </summary>
/// <typeparam name="TRequest">The type of the query request. Must implement <see cref="IQueryRequest{TResponse}" />.</typeparam>
/// <typeparam name="TResponse">The type of the response the query returns.</typeparam>
public class DefaultFirstSelector<TRequest, TResponse> : IQueryHandlerSelector<TRequest, TResponse>
  where TRequest : IQueryRequest<TResponse>
{
  /// <summary>
  ///   Selects the appropriate query handler for the given request, prioritizing handlers that implement
  ///   <see cref="IDefaultHandler{TRequest, TResponse}" />. If no handler is found, an exception is thrown.
  /// </summary>
  /// <param name="request">
  ///   The query request for which a handler is being selected. Must implement
  ///   <see cref="IQueryRequest{TResponse}" />.
  /// </param>
  /// <param name="handlers">The collection of available query handlers for the specified request type.</param>
  /// <returns>The selected query handler implementing <see cref="IQueryHandler{TRequest, TResponse}" />.</returns>
  /// <exception cref="InvalidOperationException">Thrown when no matching query handler is found.</exception>
  public IQueryHandler<TRequest, TResponse> SelectHandler(TRequest request,
    IEnumerable<IQueryHandler<TRequest, TResponse>> handlers)
  {
    var asyncMessageHandlers = handlers.ToList();
    var selected = asyncMessageHandlers.FirstOrDefault(h => h is IDefaultHandler<TRequest, TResponse>);

    return selected ?? asyncMessageHandlers.FirstOrDefault() ??
      throw new InvalidOperationException($"No handler registered for {typeof(TRequest).Name}");
  }
}