using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RGamaFelix.CqrsDispatcher.Config;
using RGamaFelix.CqrsDispatcher.Query.Handler.Selector;
using RGamaFelix.CqrsDispatcher.TestConsole.TestRequest;
using RGamaFelix.CqrsDispatcher.TestConsole.Validator;
using RGamaFelix.CqrsDispatcher.Validator.Config;

namespace RGamaFelix.CqrsDispatcher.TestConsole;

public class Program
{
  public static async Task Main()
  {
    var services = new ServiceCollection();

    // Logging and framework
    services.AddLogging(cfg =>
    {
      cfg.AddSimpleConsole();
      cfg.SetMinimumLevel(LogLevel.Warning);
    });

    services.AddCqrsDispatcherFramework();
    services.RegisterCqrsDispatcherComponents(typeof(Program).Assembly);
    services.AddScoped<IQueryHandlerSelector<SelectableQueryRequest, TestQueryResponse>, HandlerSelector>();
    services.RegisterCqrsDispatcherValidator();
    services.AddValidatorsFromAssemblyContaining(typeof(BaseCommandRequestValidator));
    var provider = services.BuildServiceProvider();
    // Create Requests
    var baseQueryRequest = new BaseQueryRequest("strValue", 1);
    var derivedQueryRequest = new DerivedQueryRequest("strValue", 2);
    var baseCommandRequest = new BaseCommandRequest("strValue", 3);
    var baseCommandRequest2 = new BaseCommandRequest("strValue", 2);
    var invalidBaseCommandRequest = new BaseCommandRequest("strValue", 4);
    var derivedCommandRequest = new DerivedCommandRequest("strValue", 4);
    var nullCommandRequest = new NullCommandRequest();
    var selectableQueryRequest1 = new SelectableQueryRequest(1);
    var selectableQueryRequest2 = new SelectableQueryRequest(2);
    // Run
    var dispatcher = provider.GetRequiredService<Dispatcher>();
    dispatcher.Publish(baseCommandRequest, Console.WriteLine);
    dispatcher.Publish(baseCommandRequest2, Console.WriteLine);
    dispatcher.Publish(invalidBaseCommandRequest, Console.WriteLine);
    dispatcher.Publish(derivedCommandRequest, Console.WriteLine);
    dispatcher.Publish(nullCommandRequest, Console.WriteLine);
    var result1 = await dispatcher.Send<BaseQueryRequest, TestQueryResponse>(baseQueryRequest);
    Console.WriteLine("Result1: " + result1);
    var result2 = await dispatcher.Send<BaseQueryRequest, TestQueryResponse>(derivedQueryRequest);
    Console.WriteLine("Result2: " + result2);
    var result3 = await dispatcher.Send<DerivedQueryRequest, TestQueryResponse>(derivedQueryRequest);
    Console.WriteLine("Result3: " + result3);
    await dispatcher.Send<SelectableQueryRequest, TestQueryResponse>(selectableQueryRequest1);
    await dispatcher.Send<SelectableQueryRequest, TestQueryResponse>(selectableQueryRequest2);
    // Give some time for all the pipelines to finish
    Thread.Sleep(2000);
  }
}
