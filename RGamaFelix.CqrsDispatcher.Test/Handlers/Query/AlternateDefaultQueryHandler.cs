using RGamaFelix.CqrsDispatcher.Query.Handler.Selector;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test.Handlers.Query;

public class AlternateDefaultQueryHandler : IDefaultQueryHandler<BaseQueryRequest, TestQueryResponse>
{
  public Task<TestQueryResponse> HandleAsync(BaseQueryRequest request, CancellationToken cancellationToken)
  {
    return Task.FromResult(new TestQueryResponse("Default" + request.StrValue + request.IntValue));
  }
}