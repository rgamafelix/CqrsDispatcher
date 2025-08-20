using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test.Handlers.Query;

public class AlternateQueryHandler : IQueryHandler<BaseQueryRequest, TestQueryResponse>
{
  public Task<TestQueryResponse> HandleAsync(BaseQueryRequest request, CancellationToken cancellationToken)
  {
    return Task.FromResult(new TestQueryResponse("Alternate" + request.StrValue + request.IntValue));
  }
}