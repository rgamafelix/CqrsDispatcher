using RGamaFelix.CqrsDispatcher.Query.Handler;

namespace RGamaFelix.CqrsDispatcher.Query.Extension.Handler;

/// <summary>
///   Defines an extension for a query handler, allowing additional behaviors to be performed during the query
///   handling process.
/// </summary>
/// <typeparam name="THandler">The type of the query handler implementing IQueryHandler.</typeparam>
/// <typeparam name="TRequest">The type of the query request implementing IQueryRequest.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the query handler.</typeparam>
public interface IQueryHandlerExtension<THandler, TRequest, TResponse>
  where THandler : IQueryHandler<TRequest, TResponse> where TRequest : IQueryRequest<TResponse>
{
  /// <summary>
  ///   Gets the order in which the query handler extension should execute relative to other extensions. A smaller
  ///   value indicates a higher priority.
  /// </summary>
  /// <remarks>
  ///   This property is optional. If not specified, the order is undefined, and the extension may execute at any
  ///   point relative to other extensions. The intended use of this property is to enable developers to control the sequence
  ///   of execution for multiple registered extensions.
  /// </remarks>
  int? Order { get; }

  /// <summary>Handles the execution of a query by providing the ability to extend or modify the handling behavior.</summary>
  /// <param name="request">The query request instance to be processed.</param>
  /// <param name="handler">The handler responsible for processing the query request.</param>
  /// <param name="next">A delegate pointing to the next function in the chain, which will process the request further.</param>
  /// <param name="cancellationToken">A token to signal the cancellation of the operation.</param>
  /// <returns>
  ///   A task representing the result of the query operation, containing the response of type
  ///   <typeparamref name="TResponse" />.
  /// </returns>
  Task<TResponse> Handle(TRequest request, THandler handler, Func<TRequest, CancellationToken, Task<TResponse>> next,
    CancellationToken cancellationToken);

  /// <summary>Determines whether the extension should execute for the specified query request.</summary>
  /// <param name="request">The query request instance that is being evaluated.</param>
  /// <returns><c>true</c> if the extension should execute; otherwise, <c>false</c>.</returns>
  bool ShouldRun(TRequest request);
}
