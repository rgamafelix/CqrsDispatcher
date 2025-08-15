using RGamaFelix.CqrsDispatcher.Query;
using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Query.Handler.Selector;
using RGamaFelix.CqrsDispatcher.TestConsole.TestRequest;

namespace RGamaFelix.CqrsDispatcher.TestConsole.FakeHandlers;

public class BaseQueryHandler : IQueryHandler<BaseQueryRequest, TestQueryResponse>
{
  public Task<TestQueryResponse> HandleAsync(BaseQueryRequest request, CancellationToken cancellationToken)
  {
    return Task.FromResult(new TestQueryResponse(request.StrValue + request.IntValue));
  }
}

public class DefaultQueryQueryHandler : IDefaultQueryHandler<BaseQueryRequest, TestQueryResponse>
{
  public Task<TestQueryResponse> HandleAsync(BaseQueryRequest request, CancellationToken cancellationToken)
  {
    return Task.FromResult(new TestQueryResponse("Default"+request.StrValue + request.IntValue));
  }
}
