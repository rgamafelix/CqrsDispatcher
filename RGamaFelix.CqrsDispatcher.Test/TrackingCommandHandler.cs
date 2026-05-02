using RGamaFelix.CqrsDispatcher.Command.Handler;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test;

public class TrackingCommandHandler(List<string> callLog) : ICommandHandler<TestCommandRequest>
{
  public Task Handle(TestCommandRequest request, CancellationToken cancellationToken)
  {
    callLog.Add("handler");

    return Task.CompletedTask;
  }
}