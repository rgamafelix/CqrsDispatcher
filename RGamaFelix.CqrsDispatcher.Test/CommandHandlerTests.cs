using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RGamaFelix.CqrsDispatcher.Command.Handler;
using RGamaFelix.CqrsDispatcher.Configuration;
using RGamaFelix.CqrsDispatcher.Exceptions;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test;

public class CommandHandlerTests
{
  [Fact]
  public async Task DispatcherShouldExecuteOnExceptionCallbackOnHandlerError()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var handler = Substitute.For<ICommandHandler<TestCommandRequest>>();

    handler.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>())
      .Returns(x => throw new ApplicationException("Test Exception"));

    services.AddScoped<ICommandHandler<TestCommandRequest>>(_ => handler);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);
    var onExceptionCallback = Substitute.For<Action<Exception>>();

    // Act
    await dispatcher.Publish(new TestCommandRequest(), onExceptionCallback);

    // Assert
    onExceptionCallback.Received(1);
  }

  [Fact]
  public async Task DispatcherShouldRethrowExceptionWhenNoExceptionCallbackIsProvided()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var handler = Substitute.For<ICommandHandler<TestCommandRequest>>();

    handler.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>())
      .Returns(x => throw new ApplicationException("Test Exception"));

    services.AddScoped<ICommandHandler<TestCommandRequest>>(_ => handler);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Assert
    await Assert.ThrowsAsync<ApplicationException>(async () => { await dispatcher.Publish(new TestCommandRequest()); });
  }

  [Fact]
  public async Task DispatcherShouldRunAllCommandHandlersWhenMultiplesCommandHandlersAreFound()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var handler1 = Substitute.For<ICommandHandler<TestCommandRequest>>();
    var handler2 = Substitute.For<ICommandHandler<TestCommandRequest>>();
    handler1.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>()).Returns(x => Task.CompletedTask);
    handler2.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>()).Returns(x => Task.CompletedTask);
    services.AddScoped<ICommandHandler<TestCommandRequest>>(_ => handler1);
    services.AddScoped<ICommandHandler<TestCommandRequest>>(_ => handler2);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act
    await dispatcher.Publish(new TestCommandRequest());

    // Assert
    await handler1.Received(1).Handle(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>());
    await handler2.Received(1).Handle(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task DispatcherShouldRunCorrectQueryHandlerWhenHandlersAreRegisteredForMultipleQueryRequests()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var handler1 = Substitute.For<ICommandHandler<TestCommandRequest>>();
    var handler2 = Substitute.For<ICommandHandler<AlternateCommandRequest>>();
    handler1.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>()).Returns(x => Task.CompletedTask);
    handler2.Handle(Arg.Any<AlternateCommandRequest>(), Arg.Any<CancellationToken>()).Returns(x => Task.CompletedTask);
    services.AddScoped<ICommandHandler<TestCommandRequest>>(_ => handler1);
    services.AddScoped<ICommandHandler<AlternateCommandRequest>>(_ => handler2);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act
    await dispatcher.Publish(new TestCommandRequest());

    // Assert
    await handler1.Received(1).Handle(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>());
    await handler2.DidNotReceive().Handle(Arg.Any<AlternateCommandRequest>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task DispatcherShouldRunWhenSingleCommandHandlerIsFound()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    services.AddCqrsDispatcherFramework();
    var handler = Substitute.For<ICommandHandler<TestCommandRequest>>();
    handler.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>()).Returns(x => Task.CompletedTask);
    services.AddScoped<ICommandHandler<TestCommandRequest>>(_ => handler);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act
    await dispatcher.Publish(new TestCommandRequest());

    // Assert
    await handler.Received(1).Handle(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task DispatcherShouldThrowExceptionWhenNoCommandHandlerIsRegistered()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    services.AddCqrsDispatcherFramework();
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act & Assert
    await Assert.ThrowsAsync<NoHandlerRegisteredException<TestCommandRequest>>(async () =>
    {
      await dispatcher.Publish(new TestCommandRequest());
    });
  }

  [Fact]
  public async Task DispatcherShouldThrowExceptionWhenRequestIsNull()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    services.AddCqrsDispatcherFramework();
    var handler = Substitute.For<ICommandHandler<TestCommandRequest>>();
    handler.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>()).Returns(x => Task.CompletedTask);
    services.AddScoped<ICommandHandler<TestCommandRequest>>(_ => handler);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentNullException>(async () => await dispatcher.Publish<TestCommandRequest>(null!));
  }
}