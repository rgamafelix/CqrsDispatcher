using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Query.Handler.Selector;

namespace RGamaFelix.CqrsDispatcher.TestConsole.TestRequest;

public class HandlerSelector : IQueryHandlerSelector<SelectableQueryRequest, TestQueryResponse>
{
  public IQueryHandler<SelectableQueryRequest, TestQueryResponse> SelectHandler(SelectableQueryRequest request,
    IEnumerable<IQueryHandler<SelectableQueryRequest, TestQueryResponse>> handlers)
  {
    return handlers.First(h => (h as ISelectQueryHandler)?.ShouldSelect == request.IntValue);
  }
}