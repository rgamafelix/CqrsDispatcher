using RGamaFelix.CqrsDispatcher.Command.Handler;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test.Handlers.Command;

public class AlternateCommandHandler : ICommandHandler<BaseCommandCommandRequest>
{
  private readonly Action _callback;

  public AlternateCommandHandler(Action callback)
  {
    _callback = callback;
  }

  public Task Handle(BaseCommandCommandRequest commandRequest, CancellationToken cancellationToken)
  {
    _callback();

    return Task.CompletedTask;
  }
}