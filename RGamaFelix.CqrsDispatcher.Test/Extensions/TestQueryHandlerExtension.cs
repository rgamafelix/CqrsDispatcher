using RGamaFelix.CqrsDispatcher.Query;
using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Query.Pipeline.Handler;

namespace RGamaFelix.CqrsDispatcher.Test.Handlers;

public class
  TestQueryHandlerExtension<THandler, TRequest, TResponse> : IQueryHandlerExtension<THandler, TRequest, TResponse>
  where THandler : IQueryHandler<TRequest, TResponse> where TRequest : IQueryRequest<TResponse>
{
  public int? Order { get; } = 0;

  public async Task<TResponse> Handle(TRequest request, THandler handler,
    Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
  {
    return await next(request, cancellationToken);
  }

  public bool ShouldRun(TRequest request)
  {
    return true;
  }
}