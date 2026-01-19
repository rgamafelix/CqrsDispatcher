using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RGamaFelix.CqrsDispatcher.Exceptions;
using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Query.Handler.Selector;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test;

public class QueryHandlerTests
{
  [Fact]
  public async Task DispatcherShouldRunCorrectQueryHandlerWhenHandlersAreRegisteredForMultipleQueryRequests()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var baseHandler = Substitute.For<IQueryHandler<TestQueryRequest, TestQueryResponse>>();

    baseHandler.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>())
      .Returns(_ => Task.FromResult(new TestQueryResponse()));

    var alternateHandler = Substitute.For<IQueryHandler<AlternateTestQueryRequest, TestQueryResponse>>();

    alternateHandler.Handle(Arg.Any<AlternateTestQueryRequest>(), Arg.Any<CancellationToken>())
      .Returns(_ => Task.FromResult(new TestQueryResponse()));

    services.AddScoped<IQueryHandler<TestQueryRequest, TestQueryResponse>>(_ => baseHandler);
    services.AddScoped<IQueryHandler<AlternateTestQueryRequest, TestQueryResponse>>(_ => alternateHandler);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    //Act
    await dispatcher.Send<TestQueryRequest, TestQueryResponse>(new TestQueryRequest());

    //Assert
    await alternateHandler.DidNotReceive().Handle(Arg.Any<AlternateTestQueryRequest>(), Arg.Any<CancellationToken>());
    await baseHandler.Received(1).Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task DispatcherShouldRunWhenOnlyOneQueryHandlerIsRegisteredForRequest()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var baseHandler = Substitute.For<IQueryHandler<TestQueryRequest, TestQueryResponse>>();

    baseHandler.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>())
      .Returns(_ => Task.FromResult(new TestQueryResponse()));

    services.AddScoped<IQueryHandler<TestQueryRequest, TestQueryResponse>>(_ => baseHandler);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    //Act
    await dispatcher.Send<TestQueryRequest, TestQueryResponse>(new TestQueryRequest());

    //Assert
    await baseHandler.Received(1).Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task DispatcherShouldRunWhenQueryHandlerIsSelected()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var baseHandler = Substitute.For<IQueryHandler<TestQueryRequest, TestQueryResponse>>();

    baseHandler.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>())
      .Returns(_ => Task.FromResult(new TestQueryResponse()));

    var alternateHandler = Substitute.For<IQueryHandler<TestQueryRequest, TestQueryResponse>>();

    alternateHandler.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>())
      .Returns(_ => Task.FromResult(new TestQueryResponse()));

    var selector = Substitute.For<IQueryHandlerSelector<TestQueryRequest, TestQueryResponse>>();

    selector.SelectHandler(Arg.Any<TestQueryRequest>(),
        Arg.Any<IEnumerable<IQueryHandler<TestQueryRequest, TestQueryResponse>>>())
      .Returns(_ => alternateHandler);

    services.AddScoped<IQueryHandler<TestQueryRequest, TestQueryResponse>>(_ => baseHandler);
    services.AddScoped<IQueryHandler<TestQueryRequest, TestQueryResponse>>(_ => alternateHandler);
    services.AddScoped<IQueryHandlerSelector<TestQueryRequest, TestQueryResponse>>(_ => selector);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    //Act
    await dispatcher.Send<TestQueryRequest, TestQueryResponse>(new TestQueryRequest());

    //Assert
    await alternateHandler.Received(1).Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>());
    await baseHandler.DidNotReceive().Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task DispatcherShouldThrowExceptionWhenMultipleQueryHandlersAreRegisteredWithoutSelector()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var baseHandler = Substitute.For<IQueryHandler<TestQueryRequest, TestQueryResponse>>();

    baseHandler.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>())
      .Returns(_ => Task.FromResult(new TestQueryResponse()));

    var alternateHandler = Substitute.For<IQueryHandler<TestQueryRequest, TestQueryResponse>>();

    alternateHandler.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>())
      .Returns(_ => Task.FromResult(new TestQueryResponse()));

    services.AddScoped<IQueryHandler<TestQueryRequest, TestQueryResponse>>(_ => baseHandler);
    services.AddScoped<IQueryHandler<TestQueryRequest, TestQueryResponse>>(_ => alternateHandler);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act & Assert
    await Assert.ThrowsAsync<NoHandlerSelectorRegisteredException<TestQueryRequest>>(async () =>
    {
      await dispatcher.Send<TestQueryRequest, TestQueryResponse>(new TestQueryRequest());
    });
  }

  [Fact]
  public async Task DispatcherShouldThrowExceptionWhenMultipleSelectorsAreRegisteredForQueryRequest()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var baseHandler = Substitute.For<IQueryHandler<TestQueryRequest, TestQueryResponse>>();

    baseHandler.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>())
      .Returns(_ => Task.FromResult(new TestQueryResponse()));

    var alternateHandler = Substitute.For<IQueryHandler<TestQueryRequest, TestQueryResponse>>();

    alternateHandler.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>())
      .Returns(_ => Task.FromResult(new TestQueryResponse()));

    var selector = Substitute.For<IQueryHandlerSelector<TestQueryRequest, TestQueryResponse>>();

    selector.SelectHandler(Arg.Any<TestQueryRequest>(),
        Arg.Any<IEnumerable<IQueryHandler<TestQueryRequest, TestQueryResponse>>>())
      .Returns(_ => alternateHandler);

    var selector2 = Substitute.For<IQueryHandlerSelector<TestQueryRequest, TestQueryResponse>>();

    selector2.SelectHandler(Arg.Any<TestQueryRequest>(),
        Arg.Any<IEnumerable<IQueryHandler<TestQueryRequest, TestQueryResponse>>>())
      .Returns(_ => alternateHandler);

    services.AddScoped<IQueryHandler<TestQueryRequest, TestQueryResponse>>(_ => baseHandler);
    services.AddScoped<IQueryHandler<TestQueryRequest, TestQueryResponse>>(_ => alternateHandler);
    services.AddScoped<IQueryHandlerSelector<TestQueryRequest, TestQueryResponse>>(_ => selector);
    services.AddScoped<IQueryHandlerSelector<TestQueryRequest, TestQueryResponse>>(_ => selector2);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act & Assert

    await Assert.ThrowsAsync<MultipleSelectorsRegisteredException<TestQueryRequest>>(async () =>
    {
      await dispatcher.Send<TestQueryRequest, TestQueryResponse>(new TestQueryRequest());
    });
  }

  [Fact]
  public async Task DispatcherShouldThrowExceptionWhenNoQueryHandlerIsRegistered()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act & Assert
    await Assert.ThrowsAsync<NoHandlerRegisteredException<TestQueryRequest>>(async () =>
    {
      await dispatcher.Send<TestQueryRequest, TestQueryResponse>(new TestQueryRequest());
    });
  }

  [Fact]
  public async Task DispatcherShouldThrowExceptionWhenNoQueryHandlerIsSelected()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var baseHandler = Substitute.For<IQueryHandler<TestQueryRequest, TestQueryResponse>>();

    baseHandler.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>())
      .Returns(_ => Task.FromResult(new TestQueryResponse()));

    var alternateHandler = Substitute.For<IQueryHandler<TestQueryRequest, TestQueryResponse>>();

    alternateHandler.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>())
      .Returns(_ => Task.FromResult(new TestQueryResponse()));

    var selector = Substitute.For<IQueryHandlerSelector<TestQueryRequest, TestQueryResponse>>();

    selector.SelectHandler(Arg.Any<TestQueryRequest>(),
        Arg.Any<IEnumerable<IQueryHandler<TestQueryRequest, TestQueryResponse>>>())
      .Returns(_ => null);

    services.AddScoped<IQueryHandler<TestQueryRequest, TestQueryResponse>>(_ => baseHandler);
    services.AddScoped<IQueryHandler<TestQueryRequest, TestQueryResponse>>(_ => alternateHandler);
    services.AddScoped<IQueryHandlerSelector<TestQueryRequest, TestQueryResponse>>(_ => selector);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act & Assert
    await Assert.ThrowsAsync<MultipleQueryHandlersRegisteredException<TestQueryRequest>>(async () =>
      await dispatcher.Send<TestQueryRequest, TestQueryResponse>(new TestQueryRequest()));
  }

  [Fact]
  public async Task DispatcherShouldThrowExceptionWhenQueryRequestIsNull()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    var baseHandler = Substitute.For<IQueryHandler<TestQueryRequest, TestQueryResponse>>();

    baseHandler.Handle(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>())
      .Returns(_ => Task.FromResult(new TestQueryResponse()));

    services.AddScoped<IQueryHandler<TestQueryRequest, TestQueryResponse>>(_ => baseHandler);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    //Act & Assert
    await Assert.ThrowsAsync<ArgumentNullException>(async () =>
    {
      await dispatcher.Send<TestQueryRequest, TestQueryResponse>(null!);
    });
  }
}
