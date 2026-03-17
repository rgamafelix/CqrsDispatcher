using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RGamaFelix.CqrsDispatcher.Command.Handler;
using RGamaFelix.CqrsDispatcher.Command.Pipeline.Handler;
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

public class TrackingHandlerExtension(List<string> callLog, string label, int order = 0, bool shouldRun = true)
  : ICommandHandlerExtension<TrackingCommandHandler, TestCommandRequest>
{
  public int? Order => order;

  public bool ShouldRun(TestCommandRequest request) => shouldRun;

  public async Task Handle(TestCommandRequest request, TrackingCommandHandler handler,
    Func<TestCommandRequest, CancellationToken, Task> next, CancellationToken cancellationToken)
  {
    callLog.Add(label);
    await next(request, cancellationToken);
  }
}

public class CommandHandlerPipelineTests
{
  [Fact]
  public async Task PipelineShouldExecuteCommandHandlerExtensionsBeforeHandler()
  {
    // Arrange
    var callLog = new List<string>();
    var services = TestHelper.CreateCleanServices();
    var handler = new TrackingCommandHandler(callLog);
    var extension = new TrackingHandlerExtension(callLog, "extension");

    services.AddScoped(
      typeof(ICommandHandlerExtension<,>).MakeGenericType(typeof(TrackingCommandHandler), typeof(TestCommandRequest)),
      _ => extension);
    services.AddScoped<ICommandHandler<TestCommandRequest>>(_ => handler);

    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act
    await dispatcher.Publish(new TestCommandRequest());

    // Assert
    Assert.Equal(["extension", "handler"], callLog);
  }

  [Fact]
  public async Task CommandHandlerExtensionShouldNotRunWhenShouldRunReturnsFalse()
  {
    // Arrange
    var callLog = new List<string>();
    var services = TestHelper.CreateCleanServices();
    var handler = new TrackingCommandHandler(callLog);
    var extension = new TrackingHandlerExtension(callLog, "extension", shouldRun: false);

    services.AddScoped(
      typeof(ICommandHandlerExtension<,>).MakeGenericType(typeof(TrackingCommandHandler), typeof(TestCommandRequest)),
      _ => extension);
    services.AddScoped<ICommandHandler<TestCommandRequest>>(_ => handler);

    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act
    await dispatcher.Publish(new TestCommandRequest());

    // Assert — extension was skipped, only handler ran
    Assert.Equal(["handler"], callLog);
  }

  [Fact]
  public async Task PipelineWithMultipleCommandHandlerExtensionsShouldRunAllExtensionsInOrder()
  {
    // Arrange
    var callLog = new List<string>();
    var services = TestHelper.CreateCleanServices();
    var handler = new TrackingCommandHandler(callLog);
    var extension1 = new TrackingHandlerExtension(callLog, "ext1", order: 1);
    var extension2 = new TrackingHandlerExtension(callLog, "ext2", order: 2);

    services.AddScoped(
      typeof(ICommandHandlerExtension<,>).MakeGenericType(typeof(TrackingCommandHandler), typeof(TestCommandRequest)),
      _ => extension1);
    services.AddScoped(
      typeof(ICommandHandlerExtension<,>).MakeGenericType(typeof(TrackingCommandHandler), typeof(TestCommandRequest)),
      _ => extension2);
    services.AddScoped<ICommandHandler<TestCommandRequest>>(_ => handler);

    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act
    await dispatcher.Publish(new TestCommandRequest());

    // Assert — lower order runs first (outermost)
    Assert.Equal(["ext1", "ext2", "handler"], callLog);
  }
}
