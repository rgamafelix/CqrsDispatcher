using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.ObserverDispacher.Test.TestRequest;

namespace RGamaFelix.ObserverDispacher.Test.FakeHandlers;

public class BaseQueryHandler : IQueryHandler<BaseQueryRequest, TestQueryResponse>
{
  public Task<TestQueryResponse> HandleAsync(BaseQueryRequest request, CancellationToken cancellationToken)
  {
    return Task.FromResult(new TestQueryResponse("Base" + request.StrValue + request.IntValue));
  }
}

public class AlternateQueryHamdler : IQueryHandler<BaseQueryRequest, TestQueryResponse>
{
  public Task<TestQueryResponse> HandleAsync(BaseQueryRequest request, CancellationToken cancellationToken)
  {
    return Task.FromResult(new TestQueryResponse("Alternate" + request.StrValue + request.IntValue));
  }
}
