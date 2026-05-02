using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RGamaFelix.CqrsDispatcher.Event.Handler;
using RGamaFelix.CqrsDispatcher.Event.Pipeline.Handler;
using RGamaFelix.CqrsDispatcher.Event.Pipeline.Request;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test;

public class EventHandlerPipelineTests
{
  [Fact]
  public async Task EventHandlerExtensionShouldNotRunWhenShouldRunReturnsFalse()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var handler = Substitute.For<IEventHandler<TestEvent>>();
    handler.Handle(Arg.Any<TestEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
    var extension = Substitute.For<IEventHandlerExtension<IEventHandler<TestEvent>, TestEvent>>();
    extension.Order.Returns(0);
    extension.ShouldRun(Arg.Any<TestEvent>()).Returns(false);
    services.AddScoped<IEventHandler<TestEvent>>(_ => handler);
    services.AddScoped<IEventHandlerExtension<IEventHandler<TestEvent>, TestEvent>>(_ => extension);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act
    await dispatcher.Notify(new TestEvent());

    // Assert
    await extension.DidNotReceive()
      .Handle(Arg.Any<TestEvent>(), Arg.Any<IEventHandler<TestEvent>>(),
        Arg.Any<Func<TestEvent, CancellationToken, Task>>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task EventHandlerExtensionShouldRunBeforeHandler()
  {
    // Arrange
    var callOrder = new List<string>();
    var services = TestHelper.CreateCleanServices();
    var handler = Substitute.For<IEventHandler<TestEvent>>();

    handler.Handle(Arg.Any<TestEvent>(), Arg.Any<CancellationToken>())
      .Returns(x =>
      {
        callOrder.Add("handler");

        return Task.CompletedTask;
      });

    var extension = Substitute.For<IEventHandlerExtension<IEventHandler<TestEvent>, TestEvent>>();
    extension.Order.Returns(0);
    extension.ShouldRun(Arg.Any<TestEvent>()).Returns(true);

    extension.Handle(Arg.Any<TestEvent>(), Arg.Any<IEventHandler<TestEvent>>(),
        Arg.Any<Func<TestEvent, CancellationToken, Task>>(), Arg.Any<CancellationToken>())
      .Returns(async x =>
      {
        callOrder.Add("extension");
        var next = x.ArgAt<Func<TestEvent, CancellationToken, Task>>(2);
        await next(x.ArgAt<TestEvent>(0), x.ArgAt<CancellationToken>(3));
      });

    services.AddScoped<IEventHandler<TestEvent>>(_ => handler);
    services.AddScoped<IEventHandlerExtension<IEventHandler<TestEvent>, TestEvent>>(_ => extension);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act
    await dispatcher.Notify(new TestEvent());

    // Assert
    Assert.Equal(["extension", "handler"], callOrder);
  }

  [Fact]
  public async Task EventRequestExtensionShouldNotRunWhenShouldRunReturnsFalse()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var handler = Substitute.For<IEventHandler<TestEvent>>();
    handler.Handle(Arg.Any<TestEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
    var extension = Substitute.For<IEventRequestExtension<TestEvent>>();
    extension.Order.Returns(0);
    extension.ShouldRun(Arg.Any<TestEvent>()).Returns(false);
    services.AddScoped<IEventHandler<TestEvent>>(_ => handler);
    services.AddScoped<IEventRequestExtension<TestEvent>>(_ => extension);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act
    await dispatcher.Notify(new TestEvent());

    // Assert
    await extension.DidNotReceive()
      .Handle(Arg.Any<TestEvent>(), Arg.Any<Func<TestEvent, CancellationToken, Task>>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task EventRequestExtensionShouldRunBeforeAllHandlers()
  {
    // Arrange
    var callOrder = new List<string>();
    var services = TestHelper.CreateCleanServices();
    var handler = Substitute.For<IEventHandler<TestEvent>>();

    handler.Handle(Arg.Any<TestEvent>(), Arg.Any<CancellationToken>())
      .Returns(x =>
      {
        callOrder.Add("handler");

        return Task.CompletedTask;
      });

    var extension = Substitute.For<IEventRequestExtension<TestEvent>>();
    extension.Order.Returns(0);
    extension.ShouldRun(Arg.Any<TestEvent>()).Returns(true);

    extension.Handle(Arg.Any<TestEvent>(), Arg.Any<Func<TestEvent, CancellationToken, Task>>(),
        Arg.Any<CancellationToken>())
      .Returns(async x =>
      {
        callOrder.Add("request-extension");
        var next = x.ArgAt<Func<TestEvent, CancellationToken, Task>>(1);
        await next(x.ArgAt<TestEvent>(0), x.ArgAt<CancellationToken>(2));
      });

    services.AddScoped<IEventHandler<TestEvent>>(_ => handler);
    services.AddScoped<IEventRequestExtension<TestEvent>>(_ => extension);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act
    await dispatcher.Notify(new TestEvent());

    // Assert
    Assert.Equal(["request-extension", "handler"], callOrder);
  }
}