using Microsoft.Extensions.DependencyInjection;
using RGamaFelix.CqrsDispatcher.Command.Handler;
using RGamaFelix.CqrsDispatcher.Command.Pipeline.Handler;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test;

public class CommandHandlerPipelineTests
{
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
  public async Task PipelineWithMultipleCommandHandlerExtensionsShouldRunAllExtensionsInOrder()
  {
    // Arrange
    var callLog = new List<string>();
    var services = TestHelper.CreateCleanServices();
    var handler = new TrackingCommandHandler(callLog);
    var extension1 = new TrackingHandlerExtension(callLog, "ext1", 1);
    var extension2 = new TrackingHandlerExtension(callLog, "ext2", 2);
    var extension3 = new TrackingHandlerExtension(callLog, "ext3", 3);

    services.AddScoped(
      typeof(ICommandHandlerExtension<,>).MakeGenericType(typeof(TrackingCommandHandler), typeof(TestCommandRequest)),
      _ => extension1);

    services.AddScoped(
      typeof(ICommandHandlerExtension<,>).MakeGenericType(typeof(TrackingCommandHandler), typeof(TestCommandRequest)),
      _ => extension3);

    services.AddScoped(
      typeof(ICommandHandlerExtension<,>).MakeGenericType(typeof(TrackingCommandHandler), typeof(TestCommandRequest)),
      _ => extension2);

    services.AddScoped<ICommandHandler<TestCommandRequest>>(_ => handler);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act
    await dispatcher.Publish(new TestCommandRequest());

    // Assert — lower order runs first (outermost)
    Assert.Equal(["ext1", "ext2", "ext3", "handler"], callLog);
  }
}
