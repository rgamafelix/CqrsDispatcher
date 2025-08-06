using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.TestConsole.TestRequest;

namespace RGamaFelix.CqrsDispatcher.TestConsole.FakeHandlers;

public class DerivedQueryHandler : IQueryHandler<DerivedQueryRequest, TestQueryResponse>
{
  public Task<TestQueryResponse> HandleAsync(DerivedQueryRequest request, CancellationToken cancellationToken)
  {
    Thread.Sleep(Random.Shared.Next(5) * 100);
    Console.WriteLine($"{GetType().Name} - {request} - HANDLING");

    return Task.FromResult(new TestQueryResponse(request.StrValue + request.IntValue));
  }
}