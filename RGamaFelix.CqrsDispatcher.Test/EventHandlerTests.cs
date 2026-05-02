using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RGamaFelix.CqrsDispatcher.Event.Handler;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test;

public class EventHandlerTests
{
  [Fact]
  public async Task DispatcherShouldExecuteOnExceptionCallbackWhenEventHandlerThrows()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var handler = Substitute.For<IEventHandler<TestEvent>>();
    handler.Handle(Arg.Any<TestEvent>(), Arg.Any<CancellationToken>())
      .Returns(x => throw new ApplicationException("Test Exception"));
    services.AddScoped<IEventHandler<TestEvent>>(_ => handler);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);
    var onExceptionCallback = Substitute.For<Action<Exception>>();

    // Act
    await dispatcher.Notify(new TestEvent(), onExceptionCallback);

    // Assert
    onExceptionCallback.Received(1);
  }

  [Fact]
  public async Task DispatcherShouldRethrowWhenNoExceptionCallbackIsProvided()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var handler = Substitute.For<IEventHandler<TestEvent>>();
    handler.Handle(Arg.Any<TestEvent>(), Arg.Any<CancellationToken>())
      .Returns(x => throw new ApplicationException("Test Exception"));
    services.AddScoped<IEventHandler<TestEvent>>(_ => handler);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act & Assert
    await Assert.ThrowsAsync<ApplicationException>(async () => { await dispatcher.Notify(new TestEvent()); });
  }

  [Fact]
  public async Task DispatcherShouldRunAllEventHandlersConcurrently()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var handler1 = Substitute.For<IEventHandler<TestEvent>>();
    var handler2 = Substitute.For<IEventHandler<TestEvent>>();
    handler1.Handle(Arg.Any<TestEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
    handler2.Handle(Arg.Any<TestEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
    services.AddScoped<IEventHandler<TestEvent>>(_ => handler1);
    services.AddScoped<IEventHandler<TestEvent>>(_ => handler2);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act
    await dispatcher.Notify(new TestEvent());

    // Assert
    await handler1.Received(1).Handle(Arg.Any<TestEvent>(), Arg.Any<CancellationToken>());
    await handler2.Received(1).Handle(Arg.Any<TestEvent>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task DispatcherShouldNotThrowWhenNoEventHandlersAreRegistered()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act & Assert — no exception expected
    await dispatcher.Notify(new TestEvent());
  }

  [Fact]
  public async Task DispatcherShouldThrowWhenEventIsNull()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentNullException>(async () => await dispatcher.Notify<TestEvent>(null!));
  }

  [Fact]
  public async Task DispatcherShouldRunCorrectEventHandlerWhenHandlersAreRegisteredForMultipleEventTypes()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var handler1 = Substitute.For<IEventHandler<TestEvent>>();
    var handler2 = Substitute.For<IEventHandler<AlternateTestEvent>>();
    handler1.Handle(Arg.Any<TestEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
    handler2.Handle(Arg.Any<AlternateTestEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
    services.AddScoped<IEventHandler<TestEvent>>(_ => handler1);
    services.AddScoped<IEventHandler<AlternateTestEvent>>(_ => handler2);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act
    await dispatcher.Notify(new TestEvent());

    // Assert
    await handler1.Received(1).Handle(Arg.Any<TestEvent>(), Arg.Any<CancellationToken>());
    await handler2.DidNotReceive().Handle(Arg.Any<AlternateTestEvent>(), Arg.Any<CancellationToken>());
  }
}
