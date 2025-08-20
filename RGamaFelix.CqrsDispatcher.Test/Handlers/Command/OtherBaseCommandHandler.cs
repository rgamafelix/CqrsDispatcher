using RGamaFelix.CqrsDispatcher.Command.Handler;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test.Handlers.Command;

public class OtherBaseCommandHandler : ICommandHandler<BaseCommandRequest>
{
  public Task Handle(BaseCommandRequest request, CancellationToken cancellationToken)
  {
    Thread.Sleep(Random.Shared.Next(5) * 100);
    Console.WriteLine($"{GetType().Name} - {request} - HANDLING");

    return Task.CompletedTask;
  }
}