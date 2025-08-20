using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test.Handlers.Query;

public class DerivedQueryHandler : IQueryHandler<DerivedQueryRequest, TestQueryResponse>
{
  public Task<TestQueryResponse> HandleAsync(DerivedQueryRequest request, CancellationToken cancellationToken)
  {
    Thread.Sleep(Random.Shared.Next(5) * 100);
    Console.WriteLine($"{GetType().Name} - {request} - HANDLING");

    return Task.FromResult(new TestQueryResponse(request.StrValue + request.IntValue));
  }
}