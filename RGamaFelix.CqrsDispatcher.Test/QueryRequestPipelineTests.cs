using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Query.Pipeline.Request;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test;

public class QueryRequestPipelineTests
{
  [Fact]
  public async Task DispatcherShouldRunOnlyQueryRequestExtensionsWithSameQueryRequest()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var extension1 = Substitute.For<IQueryRequestExtension<TestQueryRequest, TestQueryResponse>>();
    var extension2 = Substitute.For<IQueryRequestExtension<AlternateTestQueryRequest, TestQueryResponse>>();
    var handler = Substitute.For<IQueryHandler<TestQueryRequest, TestQueryResponse>>();
    extension1.ShouldRun(Arg.Any<TestQueryRequest>()).Returns(true);

    extension1.Handle(Arg.Any<TestQueryRequest>(),
        Arg.Any<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>(), Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        var next = callInfo.Arg<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>();

        return await next(callInfo.Arg<TestQueryRequest>(), callInfo.Arg<CancellationToken>());
      });

    extension2.ShouldRun(Arg.Any<AlternateTestQueryRequest>()).Returns(true);

    extension2.Handle(Arg.Any<AlternateTestQueryRequest>(),
        Arg.Any<Func<AlternateTestQueryRequest, CancellationToken, Task<TestQueryResponse>>>(),
        Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        var next = callInfo.Arg<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>();

        return await next(callInfo.Arg<TestQueryRequest>(), callInfo.Arg<CancellationToken>());
      });

    services.AddScoped(typeof(IQueryRequestExtension<TestQueryRequest, TestQueryResponse>), _ => extension1);
    services.AddScoped(typeof(IQueryRequestExtension<AlternateTestQueryRequest, TestQueryResponse>), _ => extension2);
    services.AddScoped<IQueryHandler<TestQueryRequest, TestQueryResponse>>(_ => handler);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act
    await dispatcher.Send<TestQueryRequest, TestQueryResponse>(new TestQueryRequest());

    // Assert
    Received.InOrder(() =>
    {
      extension1.Handle(Arg.Any<TestQueryRequest>(),
        Arg.Any<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>(), Arg.Any<CancellationToken>());

      handler.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>());
    });

    await extension2.DidNotReceive()
      .Handle(Arg.Any<AlternateTestQueryRequest>(),
        Arg.Any<Func<AlternateTestQueryRequest, CancellationToken, Task<TestQueryResponse>>>(),
        Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task QueryRequestExtensionsShouldNotRunWhenShouldRunReturnsFalse()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var extension1 = Substitute.For<IQueryRequestExtension<TestQueryRequest, TestQueryResponse>>();
    var extension2 = Substitute.For<IQueryRequestExtension<TestQueryRequest, TestQueryResponse>>();
    var handler = Substitute.For<IQueryHandler<TestQueryRequest, TestQueryResponse>>();
    extension1.ShouldRun(Arg.Any<TestQueryRequest>()).Returns(true);

    extension1.Handle(Arg.Any<TestQueryRequest>(),
        Arg.Any<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>(), Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        var next = callInfo.Arg<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>();

        return await next(callInfo.Arg<TestQueryRequest>(), callInfo.Arg<CancellationToken>());
      });

    extension2.ShouldRun(Arg.Any<TestQueryRequest>()).Returns(false);

    extension2.Handle(Arg.Any<TestQueryRequest>(),
        Arg.Any<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>(), Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        var next = callInfo.Arg<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>();

        return await next(callInfo.Arg<TestQueryRequest>(), callInfo.Arg<CancellationToken>());
      });

    services.AddScoped(typeof(IQueryRequestExtension<TestQueryRequest, TestQueryResponse>), _ => extension1);
    services.AddScoped(typeof(IQueryRequestExtension<TestQueryRequest, TestQueryResponse>), _ => extension2);
    services.AddScoped<IQueryHandler<TestQueryRequest, TestQueryResponse>>(_ => handler);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act
    await dispatcher.Send<TestQueryRequest, TestQueryResponse>(new TestQueryRequest());

    // Assert
    await extension1.Received(1)
      .Handle(Arg.Any<TestQueryRequest>(),
        Arg.Any<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>(), Arg.Any<CancellationToken>());

    await extension2.DidNotReceive()
      .Handle(Arg.Any<TestQueryRequest>(),
        Arg.Any<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task PipelineShouldExecuteQueryRequestExtensionsBeforeHandler()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var extension = Substitute.For<IQueryRequestExtension<TestQueryRequest, TestQueryResponse>>();
    var handler = Substitute.For<IQueryHandler<TestQueryRequest, TestQueryResponse>>();
    services.AddScoped(typeof(IQueryRequestExtension<TestQueryRequest, TestQueryResponse>), _ => extension);
    services.AddScoped<IQueryHandler<TestQueryRequest, TestQueryResponse>>(_ => handler);

    handler.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>())
      .Returns(callInfo => Task.FromResult(new TestQueryResponse()));

    extension.Handle(Arg.Any<TestQueryRequest>(),
        Arg.Any<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>(), Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        var next = callInfo.Arg<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>();

        return await next(callInfo.Arg<TestQueryRequest>(), callInfo.Arg<CancellationToken>());
      });

    extension.ShouldRun(Arg.Any<TestQueryRequest>()).Returns(true);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act
    await dispatcher.Send<TestQueryRequest, TestQueryResponse>(new TestQueryRequest());

    // Assert
    Received.InOrder(() =>
    {
      extension.Handle(Arg.Any<TestQueryRequest>(),
        Arg.Any<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>(), Arg.Any<CancellationToken>());

      handler.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>());
    });
  }

  [Fact]
  public async Task QueryRequestExtensionsWithSameOrderShouldRunTogether()
  {
    // Arrange
    var runOrder = "";
    var services = TestHelper.CreateCleanServices();
    var extension1 = Substitute.For<IQueryRequestExtension<TestQueryRequest, TestQueryResponse>>();
    var extension2 = Substitute.For<IQueryRequestExtension<TestQueryRequest, TestQueryResponse>>();
    var extension3 = Substitute.For<IQueryRequestExtension<TestQueryRequest, TestQueryResponse>>();
    var extension4 = Substitute.For<IQueryRequestExtension<TestQueryRequest, TestQueryResponse>>();
    var handler = Substitute.For<IQueryHandler<TestQueryRequest, TestQueryResponse>>();
    extension1.ShouldRun(Arg.Any<TestQueryRequest>()).Returns(true);
    extension1.Order.Returns(2);

    extension1.Handle(Arg.Any<TestQueryRequest>(),
        Arg.Any<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>(), Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        runOrder += "2";
        var next = callInfo.Arg<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>();

        return await next(callInfo.Arg<TestQueryRequest>(), callInfo.Arg<CancellationToken>());
      });

    extension2.ShouldRun(Arg.Any<TestQueryRequest>()).Returns(true);
    extension2.Order.Returns(3);

    extension2.Handle(Arg.Any<TestQueryRequest>(),
        Arg.Any<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>(), Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        runOrder += "3";
        var next = callInfo.Arg<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>();

        return await next(callInfo.Arg<TestQueryRequest>(), callInfo.Arg<CancellationToken>());
      });

    extension3.ShouldRun(Arg.Any<TestQueryRequest>()).Returns(true);
    extension3.Order.Returns(1);

    extension3.Handle(Arg.Any<TestQueryRequest>(),
        Arg.Any<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>(), Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        runOrder += "1";
        var next = callInfo.Arg<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>();

        return await next(callInfo.Arg<TestQueryRequest>(), callInfo.Arg<CancellationToken>());
      });

    extension4.ShouldRun(Arg.Any<TestQueryRequest>()).Returns(true);
    extension4.Order.Returns(2);

    extension4.Handle(Arg.Any<TestQueryRequest>(),
        Arg.Any<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>(), Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        runOrder += "2";
        var next = callInfo.Arg<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>();

        return await next(callInfo.Arg<TestQueryRequest>(), callInfo.Arg<CancellationToken>());
      });

    handler.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>())
      .Returns(callInfo =>
      {
        runOrder += "h";

        return Task.FromResult(new TestQueryResponse());
      });

    services.AddScoped(typeof(IQueryRequestExtension<TestQueryRequest, TestQueryResponse>), _ => extension1);
    services.AddScoped(typeof(IQueryRequestExtension<TestQueryRequest, TestQueryResponse>), _ => extension2);
    services.AddScoped(typeof(IQueryRequestExtension<TestQueryRequest, TestQueryResponse>), _ => extension3);
    services.AddScoped(typeof(IQueryRequestExtension<TestQueryRequest, TestQueryResponse>), _ => extension4);
    services.AddScoped<IQueryHandler<TestQueryRequest, TestQueryResponse>>(_ => handler);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);
    // Act
    await dispatcher.Send<TestQueryRequest, TestQueryResponse>(new TestQueryRequest());

    // Assert
    Assert.Equal("1223h", runOrder);
  }

  [Fact]
  public async Task PipelineWithMultipleQueryRequestExtensionsShouldRunAllExtensionsInOrder()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var extension1 = Substitute.For<QueryRequestExtensionBase<TestQueryRequest, TestQueryResponse>>();
    var extension2 = Substitute.For<IQueryRequestExtension<TestQueryRequest, TestQueryResponse>>();
    var extension3 = Substitute.For<IQueryRequestExtension<TestQueryRequest, TestQueryResponse>>();
    var handler = Substitute.For<IQueryHandler<TestQueryRequest, TestQueryResponse>>();
    extension1.ShouldRun(Arg.Any<TestQueryRequest>()).Returns(true);
    extension1.Order.Returns(2);

    extension1.Handle(Arg.Any<TestQueryRequest>(),
        Arg.Any<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>(), Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        var next = callInfo.Arg<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>();

        return await next(callInfo.Arg<TestQueryRequest>(), callInfo.Arg<CancellationToken>());
      });

    extension2.ShouldRun(Arg.Any<TestQueryRequest>()).Returns(true);
    extension2.Order.Returns(3);

    extension2.Handle(Arg.Any<TestQueryRequest>(),
        Arg.Any<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>(), Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        var next = callInfo.Arg<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>();

        return await next(callInfo.Arg<TestQueryRequest>(), callInfo.Arg<CancellationToken>());
      });

    extension3.ShouldRun(Arg.Any<TestQueryRequest>()).Returns(true);
    extension3.Order.Returns(1);

    extension3.Handle(Arg.Any<TestQueryRequest>(),
        Arg.Any<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>(), Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        var next = callInfo.Arg<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>();

        return await next(callInfo.Arg<TestQueryRequest>(), callInfo.Arg<CancellationToken>());
      });

    services.AddScoped(typeof(IQueryRequestExtension<TestQueryRequest, TestQueryResponse>), _ => extension1);
    services.AddScoped(typeof(IQueryRequestExtension<TestQueryRequest, TestQueryResponse>), _ => extension2);
    services.AddScoped(typeof(IQueryRequestExtension<TestQueryRequest, TestQueryResponse>), _ => extension3);
    services.AddScoped<IQueryHandler<TestQueryRequest, TestQueryResponse>>(_ => handler);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);
    // Act
    await dispatcher.Send<TestQueryRequest, TestQueryResponse>(new TestQueryRequest());

    // Assert
    Received.InOrder(() =>
    {
      extension3.Handle(Arg.Any<TestQueryRequest>(),
        Arg.Any<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>(), Arg.Any<CancellationToken>());

      extension1.Handle(Arg.Any<TestQueryRequest>(),
        Arg.Any<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>(), Arg.Any<CancellationToken>());

      extension2.Handle(Arg.Any<TestQueryRequest>(),
        Arg.Any<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>(), Arg.Any<CancellationToken>());

      handler.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>());
    });
  }
}
