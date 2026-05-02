using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RGamaFelix.CqrsDispatcher.Command.Handler;
using RGamaFelix.CqrsDispatcher.Command.Pipeline.Request;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test;

public class CommandRequestPipelineTests
{
  [Fact]
  public async Task CommandRequestExtensionShouldNotRunWhenShouldRunReturnsFalse()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var extension1 = Substitute.For<ICommandRequestExtension<TestCommandRequest>>();
    var extension2 = Substitute.For<ICommandRequestExtension<TestCommandRequest>>();
    var handler = Substitute.For<ICommandHandler<TestCommandRequest>>();
    extension1.ShouldRun(Arg.Any<TestCommandRequest>()).Returns(true);

    extension1.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<Func<TestCommandRequest, CancellationToken, Task>>(),
        Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        var next = callInfo.Arg<Func<TestCommandRequest, CancellationToken, Task>>();
        await next(callInfo.Arg<TestCommandRequest>(), callInfo.Arg<CancellationToken>());
      });

    extension2.ShouldRun(Arg.Any<TestCommandRequest>()).Returns(false);
    handler.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
    services.AddScoped(typeof(ICommandRequestExtension<TestCommandRequest>), _ => extension1);
    services.AddScoped(typeof(ICommandRequestExtension<TestCommandRequest>), _ => extension2);
    services.AddScoped<ICommandHandler<TestCommandRequest>>(_ => handler);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act
    await dispatcher.Publish(new TestCommandRequest());

    // Assert
    await extension1.Received(1)
      .Handle(Arg.Any<TestCommandRequest>(), Arg.Any<Func<TestCommandRequest, CancellationToken, Task>>(),
        Arg.Any<CancellationToken>());

    await extension2.DidNotReceive()
      .Handle(Arg.Any<TestCommandRequest>(), Arg.Any<Func<TestCommandRequest, CancellationToken, Task>>(),
        Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task CommandRequestExtensionsWithSameOrderShouldRunTogether()
  {
    // Arrange
    var runOrder = "";
    var services = TestHelper.CreateCleanServices();
    var extension1 = Substitute.For<ICommandRequestExtension<TestCommandRequest>>();
    var extension2 = Substitute.For<ICommandRequestExtension<TestCommandRequest>>();
    var extension3 = Substitute.For<ICommandRequestExtension<TestCommandRequest>>();
    var extension4 = Substitute.For<ICommandRequestExtension<TestCommandRequest>>();
    var handler = Substitute.For<ICommandHandler<TestCommandRequest>>();
    extension1.ShouldRun(Arg.Any<TestCommandRequest>()).Returns(true);
    extension1.Order.Returns(2);

    extension1.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<Func<TestCommandRequest, CancellationToken, Task>>(),
        Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        runOrder += "2";
        var next = callInfo.Arg<Func<TestCommandRequest, CancellationToken, Task>>();
        await next(callInfo.Arg<TestCommandRequest>(), callInfo.Arg<CancellationToken>());
      });

    extension2.ShouldRun(Arg.Any<TestCommandRequest>()).Returns(true);
    extension2.Order.Returns(3);

    extension2.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<Func<TestCommandRequest, CancellationToken, Task>>(),
        Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        runOrder += "3";
        var next = callInfo.Arg<Func<TestCommandRequest, CancellationToken, Task>>();
        await next(callInfo.Arg<TestCommandRequest>(), callInfo.Arg<CancellationToken>());
      });

    extension3.ShouldRun(Arg.Any<TestCommandRequest>()).Returns(true);
    extension3.Order.Returns(1);

    extension3.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<Func<TestCommandRequest, CancellationToken, Task>>(),
        Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        runOrder += "1";
        var next = callInfo.Arg<Func<TestCommandRequest, CancellationToken, Task>>();
        await next(callInfo.Arg<TestCommandRequest>(), callInfo.Arg<CancellationToken>());
      });

    extension4.ShouldRun(Arg.Any<TestCommandRequest>()).Returns(true);
    extension4.Order.Returns(2);

    extension4.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<Func<TestCommandRequest, CancellationToken, Task>>(),
        Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        runOrder += "2";
        var next = callInfo.Arg<Func<TestCommandRequest, CancellationToken, Task>>();
        await next(callInfo.Arg<TestCommandRequest>(), callInfo.Arg<CancellationToken>());
      });

    handler.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>())
      .Returns(callInfo =>
      {
        runOrder += "h";

        return Task.CompletedTask;
      });

    services.AddScoped(typeof(ICommandRequestExtension<TestCommandRequest>), _ => extension1);
    services.AddScoped(typeof(ICommandRequestExtension<TestCommandRequest>), _ => extension2);
    services.AddScoped(typeof(ICommandRequestExtension<TestCommandRequest>), _ => extension3);
    services.AddScoped(typeof(ICommandRequestExtension<TestCommandRequest>), _ => extension4);
    services.AddScoped<ICommandHandler<TestCommandRequest>>(_ => handler);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act
    await dispatcher.Publish(new TestCommandRequest());

    // Assert
    Assert.Equal("1223h", runOrder);
  }

  [Fact]
  public async Task DispatcherShouldRunOnlyCommandRequestExtensionsWithSameCommandRequest()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var extension1 = Substitute.For<ICommandRequestExtension<TestCommandRequest>>();
    var extension2 = Substitute.For<ICommandRequestExtension<AlternateCommandRequest>>();
    var handler = Substitute.For<ICommandHandler<TestCommandRequest>>();
    extension1.ShouldRun(Arg.Any<TestCommandRequest>()).Returns(true);

    extension1.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<Func<TestCommandRequest, CancellationToken, Task>>(),
        Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        var next = callInfo.Arg<Func<TestCommandRequest, CancellationToken, Task>>();
        await next(callInfo.Arg<TestCommandRequest>(), callInfo.Arg<CancellationToken>());
      });

    extension2.ShouldRun(Arg.Any<AlternateCommandRequest>()).Returns(true);
    handler.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
    services.AddScoped(typeof(ICommandRequestExtension<TestCommandRequest>), _ => extension1);
    services.AddScoped(typeof(ICommandRequestExtension<AlternateCommandRequest>), _ => extension2);
    services.AddScoped<ICommandHandler<TestCommandRequest>>(_ => handler);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act
    await dispatcher.Publish(new TestCommandRequest());

    // Assert
    Received.InOrder(() =>
    {
      extension1.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<Func<TestCommandRequest, CancellationToken, Task>>(),
        Arg.Any<CancellationToken>());

      handler.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>());
    });

    await extension2.DidNotReceive()
      .Handle(Arg.Any<AlternateCommandRequest>(), Arg.Any<Func<AlternateCommandRequest, CancellationToken, Task>>(),
        Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task PipelineShouldExecuteCommandRequestExtensionsBeforeHandler()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var extension = Substitute.For<ICommandRequestExtension<TestCommandRequest>>();
    var handler = Substitute.For<ICommandHandler<TestCommandRequest>>();
    services.AddScoped(typeof(ICommandRequestExtension<TestCommandRequest>), _ => extension);
    services.AddScoped<ICommandHandler<TestCommandRequest>>(_ => handler);
    handler.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

    extension.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<Func<TestCommandRequest, CancellationToken, Task>>(),
        Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        var next = callInfo.Arg<Func<TestCommandRequest, CancellationToken, Task>>();
        await next(callInfo.Arg<TestCommandRequest>(), callInfo.Arg<CancellationToken>());
      });

    extension.ShouldRun(Arg.Any<TestCommandRequest>()).Returns(true);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act
    await dispatcher.Publish(new TestCommandRequest());

    // Assert
    Received.InOrder(() =>
    {
      extension.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<Func<TestCommandRequest, CancellationToken, Task>>(),
        Arg.Any<CancellationToken>());

      handler.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>());
    });
  }

  [Fact]
  public async Task PipelineWithMultipleCommandRequestExtensionsShouldRunAllExtensionsInOrder()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var extension1 = Substitute.For<CommandRequestExtensionBase<TestCommandRequest>>();
    var extension2 = Substitute.For<ICommandRequestExtension<TestCommandRequest>>();
    var extension3 = Substitute.For<ICommandRequestExtension<TestCommandRequest>>();
    var handler = Substitute.For<ICommandHandler<TestCommandRequest>>();
    extension1.ShouldRun(Arg.Any<TestCommandRequest>()).Returns(true);
    extension1.Order.Returns(2);

    extension1.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<Func<TestCommandRequest, CancellationToken, Task>>(),
        Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        var next = callInfo.Arg<Func<TestCommandRequest, CancellationToken, Task>>();
        await next(callInfo.Arg<TestCommandRequest>(), callInfo.Arg<CancellationToken>());
      });

    extension2.ShouldRun(Arg.Any<TestCommandRequest>()).Returns(true);
    extension2.Order.Returns(3);

    extension2.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<Func<TestCommandRequest, CancellationToken, Task>>(),
        Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        var next = callInfo.Arg<Func<TestCommandRequest, CancellationToken, Task>>();
        await next(callInfo.Arg<TestCommandRequest>(), callInfo.Arg<CancellationToken>());
      });

    extension3.ShouldRun(Arg.Any<TestCommandRequest>()).Returns(true);
    extension3.Order.Returns(1);

    extension3.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<Func<TestCommandRequest, CancellationToken, Task>>(),
        Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        var next = callInfo.Arg<Func<TestCommandRequest, CancellationToken, Task>>();
        await next(callInfo.Arg<TestCommandRequest>(), callInfo.Arg<CancellationToken>());
      });

    handler.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
    services.AddScoped(typeof(ICommandRequestExtension<TestCommandRequest>), _ => extension1);
    services.AddScoped(typeof(ICommandRequestExtension<TestCommandRequest>), _ => extension2);
    services.AddScoped(typeof(ICommandRequestExtension<TestCommandRequest>), _ => extension3);
    services.AddScoped<ICommandHandler<TestCommandRequest>>(_ => handler);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act
    await dispatcher.Publish(new TestCommandRequest());

    // Assert
    Received.InOrder(() =>
    {
      extension3.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<Func<TestCommandRequest, CancellationToken, Task>>(),
        Arg.Any<CancellationToken>());

      extension1.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<Func<TestCommandRequest, CancellationToken, Task>>(),
        Arg.Any<CancellationToken>());

      extension2.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<Func<TestCommandRequest, CancellationToken, Task>>(),
        Arg.Any<CancellationToken>());

      handler.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>());
    });
  }
}