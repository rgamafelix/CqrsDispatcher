using RGamaFelix.CqrsDispatcher.Query.Handler;

namespace RGamaFelix.CqrsDispatcher.Test.TestRequest;

public class SelectQueryHandler2 : IQueryHandler<SelectableQueryRequest, TestQueryResponse>, ISelectQueryHandler
{
  public int ShouldSelect { get; } = 2;

  public Task<TestQueryResponse> HandleAsync(SelectableQueryRequest request, CancellationToken cancellationToken)
  {
    Console.WriteLine($"{GetType().Name} - {request}");

    return Task.FromResult(new TestQueryResponse("" + request.IntValue));
  }
}