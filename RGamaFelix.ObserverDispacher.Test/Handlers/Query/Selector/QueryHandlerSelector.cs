using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Query.Handler.Selector;
using RGamaFelix.ObserverDispacher.Test.TestRequest;

namespace RGamaFelix.ObserverDispacher.Test.FakeHandlers.Selector;

public class QueryHandlerSelector : IQueryHandlerSelector<BaseQueryRequest, TestQueryResponse>
{
  public IQueryHandler<BaseQueryRequest, TestQueryResponse> SelectHandler(BaseQueryRequest request,
    IEnumerable<IQueryHandler<BaseQueryRequest, TestQueryResponse>> handlers)
  {
    return handlers.First(h => h.GetType()
      .Name.Contains("Alternate"));
  }
}
