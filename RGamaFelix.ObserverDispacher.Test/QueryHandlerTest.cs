using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RGamaFelix.CqrsDispatcher;
using RGamaFelix.CqrsDispatcher.Config;
using RGamaFelix.CqrsDispatcher.Exceptions;
using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Query.Handler.Selector;
using RGamaFelix.ObserverDispacher.Test.FakeHandlers;
using RGamaFelix.ObserverDispacher.Test.FakeHandlers.Selector;
using RGamaFelix.ObserverDispacher.Test.TestRequest;

namespace RGamaFelix.ObserverDispacher.Test;

public class QueryHandlerTest
{
  private static IServiceCollection CreateCleanServices()
  {
    var services = new ServiceCollection();

    // Logging and framework
    services.AddLogging(cfg =>
    {
      cfg.AddSimpleConsole();
      cfg.SetMinimumLevel(LogLevel.Debug);
    });

    return services;
  }

  [Fact]
  public async Task DispatcherHaveOnlyOneHandlerRegisteredForSpecificQueryTest()
  {
    // Arrange
    const string expectedResult = "BasestrValue1";
    var services = CreateCleanServices();
    services.AddCqrsDispatcherFramework();
    services.AddScoped<IQueryHandler<BaseQueryRequest, TestQueryResponse>, BaseQueryHandler>();
    var provider = services.BuildServiceProvider();
    var dispatcher = provider.GetRequiredService<Dispatcher>();

    //Act
    var result = await dispatcher.Send<BaseQueryRequest, TestQueryResponse>(new BaseQueryRequest("strValue", 1));

    //Assert
    Assert.Equal(expectedResult, result.ResponseValue);
  }

  [Fact]
  public async Task DispatcherShouldSelectDefaultQueryHandlerTest()
  {
    // Arrange
    const string expectedResult = "DefaultstrValue1";
    var services = CreateCleanServices();
    services.AddCqrsDispatcherFramework();
    services.AddScoped<IQueryHandler<BaseQueryRequest, TestQueryResponse>, BaseQueryHandler>();
    services.AddScoped<IQueryHandler<BaseQueryRequest, TestQueryResponse>, DefaultQueryHandler>();
    var provider = services.BuildServiceProvider();
    var dispatcher = provider.GetRequiredService<Dispatcher>();

    //Act
    var result = await dispatcher.Send<BaseQueryRequest, TestQueryResponse>(new BaseQueryRequest("strValue", 1));

    //Assert
    Assert.Equal(expectedResult, result.ResponseValue);
  }

  [Fact]
  public async Task DispatcherShouldThrowExceptionWhenMultipleDefaultQueryHandlerIsRegisteredTest()
  {
    // Arrange
    var services = CreateCleanServices();
    services.AddCqrsDispatcherFramework();
    services.AddScoped<IQueryHandler<BaseQueryRequest, TestQueryResponse>, DefaultQueryHandler>();
    services.AddScoped<IQueryHandler<BaseQueryRequest, TestQueryResponse>, AlternateDefaultQueryHandler>();
    var provider = services.BuildServiceProvider();
    var dispatcher = provider.GetRequiredService<Dispatcher>();

    // Act
    var exception = await Assert.ThrowsAsync<MultipleQueryHandlersRegisteredException<BaseQueryRequest>>(async () =>
    {
      await dispatcher.Send<BaseQueryRequest, TestQueryResponse>(new BaseQueryRequest("strValue", 1));
    });
  }

  [Fact]
  public async Task DispatcherShouldSelectBasendonSelectorQueryHandlerTest()
  {
    // Arrange
    const string expectedResult = "AlternatestrValue1";
    var services = CreateCleanServices();
    services.AddCqrsDispatcherFramework();
    services.AddScoped<IQueryHandler<BaseQueryRequest, TestQueryResponse>, BaseQueryHandler>();
    services.AddScoped<IQueryHandler<BaseQueryRequest, TestQueryResponse>, AlternateQueryHandler>();
    services.AddScoped<IQueryHandlerSelector<BaseQueryRequest, TestQueryResponse>, QueryHandlerSelector>();
    var provider = services.BuildServiceProvider();
    var dispatcher = provider.GetRequiredService<Dispatcher>();

    //Act
    var result = await dispatcher.Send<BaseQueryRequest, TestQueryResponse>(new BaseQueryRequest("strValue", 1));

    //Assert
    Assert.Equal(expectedResult, result.ResponseValue);
  }

  [Fact]
  public async Task DispatcherShouldThrowExceptionWhenMultipleQueryHandlerIsRegisteredTest()
  {
    // Arrange
    var services = CreateCleanServices();
    services.AddCqrsDispatcherFramework();
    services.AddScoped<IQueryHandler<BaseQueryRequest, TestQueryResponse>, BaseQueryHandler>();
    services.AddScoped<IQueryHandler<BaseQueryRequest, TestQueryResponse>, AlternateQueryHandler>();
    var provider = services.BuildServiceProvider();
    var dispatcher = provider.GetRequiredService<Dispatcher>();

    // Act
    var exception = await Assert.ThrowsAsync<MultipleQueryHandlersRegisteredException<BaseQueryRequest>>(async () =>
    {
      await dispatcher.Send<BaseQueryRequest, TestQueryResponse>(new BaseQueryRequest("strValue", 1));
    });
  }

  [Fact]
  public async Task DispatcherShouldThrowExceptionWhenNoQueryHandlerIsRegisteredTest()
  {
    // Arrange
    var services = CreateCleanServices();
    services.AddCqrsDispatcherFramework();
    var provider = services.BuildServiceProvider();
    var dispatcher = provider.GetRequiredService<Dispatcher>();

    //Act
    var exception = await Assert.ThrowsAsync<NoHandlerRegisteredException<BaseQueryRequest>>(async () =>
    {
      await dispatcher.Send<BaseQueryRequest, TestQueryResponse>(new BaseQueryRequest("strValue", 1));
    });
  }
}
