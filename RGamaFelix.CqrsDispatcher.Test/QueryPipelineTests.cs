using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Query.Pipeline.Handler;

namespace RGamaFelix.CqrsDispatcher.Test.TestRequest;

public class QueryPipelineTests
{
  [Fact]
  public async Task PipelineShouldExecuteQueryHandlerExtensionsBeforeHandler()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var handler = Substitute.For<IQueryHandler<TestQueryRequest, TestQueryResponse>>();

    var extension = Substitute
      .For<IQueryHandlerExtension<IQueryHandler<TestQueryRequest, TestQueryResponse>, TestQueryRequest,
        TestQueryResponse>>();

    var handlerServiceType = typeof(IQueryHandler<TestQueryRequest, TestQueryResponse>);

    services.AddScoped(
      typeof(IQueryHandlerExtension<,,>).MakeGenericType(handlerServiceType, typeof(TestQueryRequest),
        typeof(TestQueryResponse)), _ => extension);

    services.AddScoped<IQueryHandler<TestQueryRequest, TestQueryResponse>>(_ => handler);

    handler.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>())
      .Returns(callInfo => Task.FromResult(new TestQueryResponse()));

    extension.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<IQueryHandler<TestQueryRequest, TestQueryResponse>>(),
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
      extension.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<IQueryHandler<TestQueryRequest, TestQueryResponse>>(),
        Arg.Any<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>(), Arg.Any<CancellationToken>());

      handler.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>());
    });
  }
}