using RGamaFelix.CqrsDispatcher.Command.Handler;
using RGamaFelix.ObserverDispacher.Test.TestRequest;

namespace RGamaFelix.ObserverDispacher.Test.FakeHandlers;

public class BaseCommandHandler : ICommandHandler<BaseCommandRequest>
{
  public Task Handle(BaseCommandRequest request, CancellationToken cancellationToken)
  {
    Thread.Sleep(Random.Shared.Next(5) * 100);
    Console.WriteLine($"{GetType().Name} - {request} - HANDLING");

    return Task.CompletedTask;
  }
}
