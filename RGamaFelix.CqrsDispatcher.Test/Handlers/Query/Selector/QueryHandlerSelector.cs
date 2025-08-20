using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Query.Handler.Selector;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test.Handlers.Query.Selector;

public class QueryHandlerSelector : IQueryHandlerSelector<BaseQueryRequest, TestQueryResponse>
{
  public IQueryHandler<BaseQueryRequest, TestQueryResponse> SelectHandler(BaseQueryRequest request,
    IEnumerable<IQueryHandler<BaseQueryRequest, TestQueryResponse>> handlers)
  {
    return handlers.First(h => h.GetType()
      .Name.Contains("Alternate"));
  }
}