using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test.Handlers.Query;

public class BaseQueryHandler : IQueryHandler<BaseQueryRequest, TestQueryResponse>
{
  public Task<TestQueryResponse> HandleAsync(BaseQueryRequest request, CancellationToken cancellationToken)
  {
    return Task.FromResult(new TestQueryResponse("Base" + request.StrValue + request.IntValue));
  }
}