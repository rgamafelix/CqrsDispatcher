using Microsoft.Extensions.DependencyInjection;
using RGamaFelix.CqrsDispatcher.Config;
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
  public async Task DispatcherHaveOnlyOneHandlerRegisteredForSpecificQueryTest()
  {
    // Arrange
    const string expectedResult = "BasestrValue1";
    var services = TestHelper.CreateCleanServices();
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
  public async Task DispatcherShouldSelectBasendonSelectorQueryHandlerTest()
  {
    // Arrange
    const string expectedResult = "AlternatestrValue1";
    var services = TestHelper.CreateCleanServices();
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
  public async Task DispatcherShouldSelectDefaultQueryHandlerTest()
  {
    // Arrange
    const string expectedResult = "DefaultstrValue1";
    var services = TestHelper.CreateCleanServices();
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
    var services = TestHelper.CreateCleanServices();
    services.AddCqrsDispatcherFramework();
    services.AddScoped<IQueryHandler<BaseQueryRequest, TestQueryResponse>, DefaultQueryHandler>();
    services.AddScoped<IQueryHandler<BaseQueryRequest, TestQueryResponse>, AlternateDefaultQueryHandler>();
    var provider = services.BuildServiceProvider();
    var dispatcher = provider.GetRequiredService<Dispatcher>();

    // Act
    await Assert.ThrowsAsync<MultipleQueryHandlersRegisteredException<BaseQueryRequest>>(async () =>
    {
      await dispatcher.Send<BaseQueryRequest, TestQueryResponse>(new BaseQueryRequest("strValue", 1));
    });
  }

  [Fact]
  public async Task DispatcherShouldThrowExceptionWhenMultipleQueryHandlerIsRegisteredTest()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    services.AddCqrsDispatcherFramework();
    services.AddScoped<IQueryHandler<BaseQueryRequest, TestQueryResponse>, BaseQueryHandler>();
    services.AddScoped<IQueryHandler<BaseQueryRequest, TestQueryResponse>, AlternateQueryHandler>();
    var provider = services.BuildServiceProvider();
    var dispatcher = provider.GetRequiredService<Dispatcher>();

    // Act
    await Assert.ThrowsAsync<MultipleQueryHandlersRegisteredException<BaseQueryRequest>>(async () =>
    {
      await dispatcher.Send<BaseQueryRequest, TestQueryResponse>(new BaseQueryRequest("strValue", 1));
    });
  }

  [Fact]
  public async Task DispatcherShouldThrowExceptionWhenNoQueryHandlerIsRegisteredTest()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    services.AddCqrsDispatcherFramework();
    var provider = services.BuildServiceProvider();
    var dispatcher = provider.GetRequiredService<Dispatcher>();

    //Act
    await Assert.ThrowsAsync<NoHandlerRegisteredException<BaseQueryRequest>>(async () =>
    {
      await dispatcher.Send<BaseQueryRequest, TestQueryResponse>(new BaseQueryRequest("strValue", 1));
    });
  }
}
