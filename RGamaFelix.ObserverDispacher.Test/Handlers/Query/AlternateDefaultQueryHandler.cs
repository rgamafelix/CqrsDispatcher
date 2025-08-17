using RGamaFelix.CqrsDispatcher.Query.Handler.Selector;
using RGamaFelix.ObserverDispacher.Test.TestRequest;

namespace RGamaFelix.ObserverDispacher.Test.FakeHandlers;

public class AlternateDefaultQueryHandler : IDefaultQueryHandler<BaseQueryRequest, TestQueryResponse>
{
  public Task<TestQueryResponse> HandleAsync(BaseQueryRequest request, CancellationToken cancellationToken)
  {
    return Task.FromResult(new TestQueryResponse("Default"+request.StrValue + request.IntValue));
  }
}