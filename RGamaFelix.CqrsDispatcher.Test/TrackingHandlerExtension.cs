using RGamaFelix.CqrsDispatcher.Command.Pipeline.Handler;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test;

public class TrackingHandlerExtension(List<string> callLog, string label, int order = 0, bool shouldRun = true)
  : ICommandHandlerExtension<TrackingCommandHandler, TestCommandRequest>
{
  public int? Order => order;

  public async Task Handle(TestCommandRequest request, TrackingCommandHandler handler,
    Func<TestCommandRequest, CancellationToken, Task> next, CancellationToken cancellationToken)
  {
    callLog.Add(label);
    await next(request, cancellationToken);
  }

  public bool ShouldRun(TestCommandRequest request)
  {
    return shouldRun;
  }
}