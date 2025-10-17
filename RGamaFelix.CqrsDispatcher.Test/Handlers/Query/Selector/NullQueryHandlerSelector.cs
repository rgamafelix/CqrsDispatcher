using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Query.Handler.Selector;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test.Handlers.Query.Selector;

public class NullQueryHandlerSelector : IQueryHandlerSelector<QueryRequest, TestQueryResponse>
{
  public IQueryHandler<QueryRequest, TestQueryResponse>? SelectHandler(QueryRequest queryRequest,
    IEnumerable<IQueryHandler<QueryRequest, TestQueryResponse>> handlers)
  {
    return null;
  }
}