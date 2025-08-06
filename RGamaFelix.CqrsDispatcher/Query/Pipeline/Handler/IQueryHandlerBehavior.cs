using RGamaFelix.CqrsDispatcher.Query.Handler;

namespace RGamaFelix.CqrsDispatcher.Query.Pipeline.Handler;

public interface IQueryHandlerBehavior<THandler, TRequest, TResponse>
  where THandler : IQueryHandler<TRequest, TResponse> where TRequest : IQueryRequest<TResponse>
{
  int? Order { get; }

  Task<TResponse> Handle(TRequest request, THandler handler, Func<TRequest, CancellationToken, Task<TResponse>> next,
    CancellationToken cancellationToken);

  bool ShouldRun(TRequest request);
}