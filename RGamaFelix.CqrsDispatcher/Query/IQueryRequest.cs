namespace RGamaFelix.CqrsDispatcher.Query;

/// <summary>Represents a query request that produces a response of the specified type.</summary>
/// <typeparam name="TResponse">The type of the response produced by the query request.</typeparam>
public interface IQueryRequest<TResponse> : IRequest;