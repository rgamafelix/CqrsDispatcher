namespace RGamaFelix.CqrsDispatcher.Query.Handler.Selector;

/// <summary>Represents a default handler for processing a specific type of query request.</summary>
/// <typeparam name="TRequest">
///   The type of the query request that the handler processes. Must implement
///   <see cref="IQueryRequest{TResponse}" />.
/// </typeparam>
/// <typeparam name="TResponse">The type of the response the query handler produces.</typeparam>
public interface IDefaultHandler<TRequest, TResponse>:IQueryHandler<TRequest, TResponse>
  where TRequest : IQueryRequest<TResponse>
{
}
