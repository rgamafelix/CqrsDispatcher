using RGamaFelix.CqrsDispatcher.Query.Handler;

namespace RGamaFelix.ObserverDispacher.Test.TestRequest;

public class SelectQueryHandler1 : IQueryHandler<SelectableQueryRequest, TestQueryResponse>, ISelectQueryHandler
{
  public int ShouldSelect { get; } = 1;

  public Task<TestQueryResponse> HandleAsync(SelectableQueryRequest request, CancellationToken cancellationToken)
  {
    Console.WriteLine($"{GetType().Name} - {request}");

    return Task.FromResult(new TestQueryResponse("" + request.IntValue));
  }
}