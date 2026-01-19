using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Query.Pipeline.Handler;
using RGamaFelix.CqrsDispatcher.Test.Handlers;

namespace RGamaFelix.CqrsDispatcher.Test.TestRequest;

public class QueryHandlerPipelineTests
{
  [Fact]
  public async Task QueryHandlerPipelineShouldExecuteExtensionsBeforeHandler()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var handler = Substitute.For<IQueryHandler<TestQueryRequest, TestQueryResponse>>();

    var extension = Substitute
      .For<IQueryHandler<TestQueryRequest, TestQueryResponse>, TestQueryRequest, TestQueryResponse>();

    services.AddScoped(
      typeof(IQueryHandlerExtension<IQueryHandler<TestQueryRequest, TestQueryResponse>, TestQueryRequest,
        TestQueryResponse>), _ => extension);

    services.AddScoped(typeof(IQueryHandler<TestQueryRequest, TestQueryResponse>), _ => handler);

    extension.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        var next = callInfo.Arg<Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>>>();

        return await next(callInfo.Arg<TestQueryRequest>(), callInfo.Arg<CancellationToken>());
      });

    handler.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>())
      .Returns(callInfo => Task.FromResult(new TestQueryResponse()));

    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act
    await dispatcher.Send<TestQueryRequest, TestQueryResponse>(new TestQueryRequest());

    // Assert
    Received.InOrder(() =>
    {
      extension.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>());

      handler.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>());
    });
  }
}
