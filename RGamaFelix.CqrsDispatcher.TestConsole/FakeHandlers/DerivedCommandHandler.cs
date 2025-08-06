using RGamaFelix.CqrsDispatcher.Command.Handler;
using RGamaFelix.CqrsDispatcher.TestConsole.TestRequest;

namespace RGamaFelix.CqrsDispatcher.TestConsole.FakeHandlers;

public class DerivedCommandHandler : ICommandHandler<DerivedCommandRequest>
{
  public Task Handle(DerivedCommandRequest request, CancellationToken cancellationToken)
  {
    Thread.Sleep(Random.Shared.Next(5) * 100);
    Console.WriteLine($"{GetType().Name} - {request} - HANDLING");

    return Task.CompletedTask;
  }
}