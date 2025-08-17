using RGamaFelix.CqrsDispatcher.Exceptions;

namespace RGamaFelix.CqrsDispatcher.Query.Handler.Selector;

public class DefaultSelector<TRequest, TResponse> : IQueryHandlerSelector<TRequest, TResponse>
  where TRequest : IQueryRequest<TResponse>
{
  public IQueryHandler<TRequest, TResponse> SelectHandler(TRequest request,
    IEnumerable<IQueryHandler<TRequest, TResponse>> handlers)
  {
    var asyncMessageHandlers = handlers.ToList();

    switch (asyncMessageHandlers.Count)
    {
      case 0:
        throw new NoHandlerRegisteredException<TRequest>();
      case > 1:
      {
        var selected = asyncMessageHandlers.Where(h => h is IDefaultQueryHandler<TRequest, TResponse>)
          .ToList();

        return selected.Count != 1 ? throw new MultipleQueryHandlersRegisteredException<TRequest>() : selected.First();
      }
      default:
        return asyncMessageHandlers.First();
    }
  }
}
