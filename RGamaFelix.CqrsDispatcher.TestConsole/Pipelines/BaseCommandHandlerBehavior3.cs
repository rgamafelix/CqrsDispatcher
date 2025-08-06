using RGamaFelix.CqrsDispatcher.Command.Pipeline.Handler;
using RGamaFelix.CqrsDispatcher.TestConsole.FakeHandlers;
using RGamaFelix.CqrsDispatcher.TestConsole.TestRequest;

namespace RGamaFelix.CqrsDispatcher.TestConsole.Pipelines;

public class BaseCommandHandlerBehavior3 : ICommandHandlerBehavior<BaseCommandHandler2, BaseCommandRequest>
{
  public int? Order => 0;

  public async Task Handle(BaseCommandRequest request, BaseCommandHandler2 handler,
    Func<BaseCommandRequest, CancellationToken, Task> next, CancellationToken cancellationToken)
  {
    Console.WriteLine($"{GetType().Name} - {request} - BEFORE");
    Thread.Sleep(Random.Shared.Next(5) * 100);
    await next(request, cancellationToken);
    Console.WriteLine($"{GetType().Name} - {request} - AFTER");
  }

  public bool ShouldRun(BaseCommandRequest request)
  {
    return true;
  }
}
