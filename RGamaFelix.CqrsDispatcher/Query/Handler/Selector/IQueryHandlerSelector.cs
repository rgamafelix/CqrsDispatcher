using RGamaFelix.CqrsDispatcher.Command;

namespace RGamaFelix.CqrsDispatcher.Query.Handler.Selector;

/// <summary>
///   Defines a mechanism to select an appropriate query handler for a given query request from a collection of
///   available handlers.
/// </summary>
/// <typeparam name="TRequest">The type of the query request. Must implement <see cref="ICommandRequest" />.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the query handler.</typeparam>
public interface IQueryHandlerSelector<TRequest, TResponse> where TRequest : IQueryRequest<TResponse>
{
  /// <summary>
  ///   Selects and returns the appropriate query handler from a collection of handlers based on the specified query
  ///   request.
  /// </summary>
  /// <param name="request">The query request for which an appropriate handler is to be selected.</param>
  /// <param name="handlers">A collection of potential handlers that can handle the specified query request.</param>
  /// <returns>
  ///   The selected query handler capable of handling the specified query request, or null if no suitable handler is
  ///   found.
  /// </returns>
  IQueryHandler<TRequest, TResponse>? SelectHandler(TRequest request,
    IEnumerable<IQueryHandler<TRequest, TResponse>> handlers);
}