using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test.Handlers.Query;

public class BaseQueryHandler : IQueryHandler<QueryRequest, TestQueryResponse>
{
  public Task<TestQueryResponse> HandleAsync(QueryRequest queryRequest, CancellationToken cancellationToken)
  {
    return Task.FromResult(new TestQueryResponse("Base" + queryRequest.StrValue + queryRequest.IntValue));
  }
}