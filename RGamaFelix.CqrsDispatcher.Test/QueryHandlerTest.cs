using Microsoft.Extensions.DependencyInjection;
using RGamaFelix.CqrsDispatcher.Configuration;
using RGamaFelix.CqrsDispatcher.Exceptions;
using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Query.Handler.Selector;
using RGamaFelix.CqrsDispatcher.Test.Handlers.Query;
using RGamaFelix.CqrsDispatcher.Test.Handlers.Query.Selector;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test;

public class QueryHandlerTest
{
  [Fact]
  public async Task DispatcherShouldRunWhenHandlerIsSelected()
  {
    // Arrange
    const string expectedResult = "AlternatestrValue1";
    var services = TestHelper.CreateCleanServices();
    services.AddCqrsDispatcherFramework();
    services.AddScoped<IQueryHandler<QueryRequest, TestQueryResponse>, BaseQueryHandler>();
    services.AddScoped<IQueryHandler<QueryRequest, TestQueryResponse>, AlternateQueryHandler>();
    services.AddScoped<IQueryHandlerSelector<QueryRequest, TestQueryResponse>, QueryHandlerSelector>();
    var provider = services.BuildServiceProvider();
    var dispatcher = provider.GetRequiredService<Dispatcher>();

    //Act
    var result = await dispatcher.Send<QueryRequest, TestQueryResponse>(new QueryRequest("strValue", 1));

    //Assert
    Assert.Equal(expectedResult, result.ResponseValue);
  }

  [Fact]
  public async Task DispatcherShouldRunWhenOnlyOneHandlerIsRegisteredForRequest()
  {
    // Arrange
    const string expectedResult = "BasestrValue1";
    var services = TestHelper.CreateCleanServices();
    services.AddCqrsDispatcherFramework();
    services.AddScoped<IQueryHandler<QueryRequest, TestQueryResponse>, BaseQueryHandler>();
    var provider = services.BuildServiceProvider();
    var dispatcher = provider.GetRequiredService<Dispatcher>();

    //Act
    var result = await dispatcher.Send<QueryRequest, TestQueryResponse>(new QueryRequest("strValue", 1));

    //Assert
    Assert.Equal(expectedResult, result.ResponseValue);
  }

  [Fact]
  public async Task DispatcherShouldThrowExceptionWhenMultipleQueryHandlersAreRegisteredWithoutSelector()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    services.AddCqrsDispatcherFramework();
    services.AddScoped<IQueryHandler<QueryRequest, TestQueryResponse>, BaseQueryHandler>();
    services.AddScoped<IQueryHandler<QueryRequest, TestQueryResponse>, AlternateQueryHandler>();
    var provider = services.BuildServiceProvider();
    var dispatcher = provider.GetRequiredService<Dispatcher>();

    // Act
    await Assert.ThrowsAsync<NoHandlerSelectorRegisteredException<QueryRequest>>(async () =>
    {
      await dispatcher.Send<QueryRequest, TestQueryResponse>(new QueryRequest("strValue", 1));
    });
  }

  [Fact]
  public async Task DispatcherShouldThrowExceptionWhenMultipleSelectorsAreRegisteredForRequest()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    services.AddCqrsDispatcherFramework();
    services.AddScoped<IQueryHandler<QueryRequest, TestQueryResponse>, BaseQueryHandler>();
    services.AddScoped<IQueryHandler<QueryRequest, TestQueryResponse>, AlternateQueryHandler>();
    services.AddScoped<IQueryHandlerSelector<QueryRequest, TestQueryResponse>, QueryHandlerSelector>();
    services.AddScoped<IQueryHandlerSelector<QueryRequest, TestQueryResponse>, NullQueryHandlerSelector>();
    var provider = services.BuildServiceProvider();
    var dispatcher = provider.GetRequiredService<Dispatcher>();

    await Assert.ThrowsAsync<MultipleSelectorsRegisteredException<QueryRequest>>(async () =>
    {
      await dispatcher.Send<QueryRequest, TestQueryResponse>(new QueryRequest("strValue", 1));
    });
  }

  [Fact]
  public async Task DispatcherShouldThrowExceptionWhenNoQueryHandlerIsRegistered()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    services.AddCqrsDispatcherFramework();
    var provider = services.BuildServiceProvider();
    var dispatcher = provider.GetRequiredService<Dispatcher>();

    //Act
    await Assert.ThrowsAsync<NoHandlerRegisteredException<QueryRequest>>(async () =>
    {
      await dispatcher.Send<QueryRequest, TestQueryResponse>(new QueryRequest("strValue", 1));
    });
  }

  [Fact]
  public async Task DispatcherShouldThrowExceptionWhenNoQueryHandlerIsSelected()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    services.AddCqrsDispatcherFramework();
    services.AddScoped<IQueryHandler<QueryRequest, TestQueryResponse>, BaseQueryHandler>();
    services.AddScoped<IQueryHandler<QueryRequest, TestQueryResponse>, AlternateQueryHandler>();
    services.AddScoped<IQueryHandlerSelector<QueryRequest, TestQueryResponse>, NullQueryHandlerSelector>();
    var provider = services.BuildServiceProvider();
    var dispatcher = provider.GetRequiredService<Dispatcher>();

    // Act
    await Assert.ThrowsAsync<MultipleQueryHandlersRegisteredException<QueryRequest>>(async () =>
      await dispatcher.Send<QueryRequest, TestQueryResponse>(new QueryRequest("strValue", 1)));
  }

  [Fact]
  public async Task DispatcherShouldThrowExceptionWhenRequestIsNull()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    services.AddCqrsDispatcherFramework();
    var provider = services.BuildServiceProvider();
    var dispatcher = provider.GetRequiredService<Dispatcher>();

    //Act
    await Assert.ThrowsAsync<ArgumentNullException>(async () =>
    {
      await dispatcher.Send<QueryRequest, TestQueryResponse>(null!);
    });
  }
}