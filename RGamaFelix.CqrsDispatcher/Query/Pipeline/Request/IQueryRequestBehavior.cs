namespace RGamaFelix.CqrsDispatcher.Query.Pipeline.Request;

public interface IQueryRequestBehavior<TRequest, TResponse>
  where TRequest : IQueryRequest<TResponse>
{
  int? Order { get; }

  Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next,
    CancellationToken cancellationToken);

  bool ShouldRun(TRequest request);
}