using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test.Handlers.Query;

public class AlternateQueryHandler : IQueryHandler<QueryRequest, TestQueryResponse>
{
  public Task<TestQueryResponse> HandleAsync(QueryRequest queryRequest, CancellationToken cancellationToken)
  {
    return Task.FromResult(new TestQueryResponse("Alternate" + queryRequest.StrValue + queryRequest.IntValue));
  }
}