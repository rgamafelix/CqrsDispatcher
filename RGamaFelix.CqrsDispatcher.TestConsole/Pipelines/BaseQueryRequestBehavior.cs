using RGamaFelix.CqrsDispatcher.Query.Pipeline.Request;
using RGamaFelix.CqrsDispatcher.TestConsole.TestRequest;

namespace RGamaFelix.CqrsDispatcher.TestConsole.Pipelines;

public class BaseQueryRequestBehavior : IQueryRequestBehavior<BaseQueryRequest, TestQueryResponse>
{
  public int? Order => 0;

  public async Task<TestQueryResponse> Handle(BaseQueryRequest request,
    Func<BaseQueryRequest, CancellationToken, Task<TestQueryResponse>> next, CancellationToken cancellationToken)
  {
    Console.WriteLine($"{GetType().Name} - {request} - BEFORE");
    Thread.Sleep(Random.Shared.Next(5) * 100);
    var result = await next(request, cancellationToken);
    Console.WriteLine($"{GetType().Name} - {request} - AFTER");

    return result;
  }

  public bool ShouldRun(BaseQueryRequest request)
  {
    return true;
  }
}