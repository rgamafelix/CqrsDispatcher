using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test.Handlers.Query;

public class DerivedQueryHandler : IQueryHandler<DerivedQueryRequest, TestQueryResponse>
{
  public Task<TestQueryResponse> HandleAsync(DerivedQueryRequest queryRequest, CancellationToken cancellationToken)
  {
    Thread.Sleep(Random.Shared.Next(5) * 100);
    Console.WriteLine($"{GetType().Name} - {queryRequest} - HANDLING");

    return Task.FromResult(new TestQueryResponse(queryRequest.StrValue + queryRequest.IntValue));
  }
}