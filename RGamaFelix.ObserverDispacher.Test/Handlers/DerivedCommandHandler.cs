using RGamaFelix.CqrsDispatcher.Command.Handler;
using RGamaFelix.ObserverDispacher.Test.TestRequest;

namespace RGamaFelix.ObserverDispacher.Test.FakeHandlers;

public class DerivedCommandHandler : ICommandHandler<DerivedCommandRequest>
{
  public Task Handle(DerivedCommandRequest request, CancellationToken cancellationToken)
  {
    Thread.Sleep(Random.Shared.Next(5) * 100);
    Console.WriteLine($"{GetType().Name} - {request} - HANDLING");

    return Task.CompletedTask;
  }
}