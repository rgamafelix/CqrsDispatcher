namespace RGamaFelix.CqrsDispatcher.Query.Handler.Selector;

/// <summary>
///   Defines a mechanism to select an appropriate query handler for a given query request from a collection of
///   available handlers.
/// </summary>
/// <typeparam name="TRequest">The type of the query request. Must implement <see cref="IQueryRequest{TResponse}" />.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the query handler.</typeparam>
public interface IQueryHandlerSelector<TRequest, TResponse>
  where TRequest : IQueryRequest<TResponse>
{
  /// <summary>Selects the appropriate query handler for the provided request from a collection of handlers.</summary>
  /// <param name="request">The query request for which a handler is to be selected.</param>
  /// <param name="handlers">The collection of potential query handlers.</param>
  /// <returns>Returns the selected query handler capable of processing the specified request.</returns>
  IQueryHandler<TRequest, TResponse> SelectHandler(TRequest request,
    IEnumerable<IQueryHandler<TRequest, TResponse>> handlers);
}