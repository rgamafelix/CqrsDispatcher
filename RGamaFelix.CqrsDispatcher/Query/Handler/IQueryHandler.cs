using RGamaFelix.CqrsDispatcher.Command;

namespace RGamaFelix.CqrsDispatcher.Query.Handler;

/// <summary>
///   Defines a handler for processing requests of type <typeparamref name="TRequest" /> and producing a response of
///   type <typeparamref name="TResponse" />.
/// </summary>
/// <typeparam name="TRequest">The type of the request message. Must implement <see cref="IQueryRequest{TResponse}" />.</typeparam>
/// <typeparam name="TResponse">The type of the response message.</typeparam>
public interface IQueryHandler<TRequest, TResponse> where TRequest : IQueryRequest<TResponse>
{
  /// <summary>Handles the given query request asynchronously and returns the corresponding response.</summary>
  /// <param name="request">The query request of type <typeparamref name="TRequest" /> to process.</param>
  /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
  /// <returns>
  ///   A task representing the asynchronous operation that resolves to the response of type
  ///   <typeparamref name="TResponse" />.
  /// </returns>
  Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}
