using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using RGamaFelix.CqrsDispatcher.Command.Handler;
using RGamaFelix.CqrsDispatcher.Command.Pipeline.Handler;
using RGamaFelix.CqrsDispatcher.Command.Pipeline.Request;
using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Query.Handler.Selector;
using RGamaFelix.CqrsDispatcher.Query.Pipeline.Handler;
using RGamaFelix.CqrsDispatcher.Query.Pipeline.Request;

namespace RGamaFelix.CqrsDispatcher.Config;

// CqrsDispatcherConfiguration.cs

public static class CqrsDispatcherConfiguration
{
  public static IServiceCollection AddCqrsDispatcherFramework(this IServiceCollection services)
  {
    // Core mediator
    services.AddSingleton<Dispatcher>();

    // Handler selector
    services.AddScoped(typeof(IQueryHandlerSelector<,>), typeof(DefaultFirstSelector<,>));

    return services;
  }

  public static IServiceCollection RegisterCqrsDispatcherComponents(this IServiceCollection services,
    params Assembly[] assemblies)
  {
    services.Scan(scan => scan.FromAssemblies(assemblies)
      .AddClasses(classes => classes.AssignableToAny(typeof(IQueryHandler<,>), typeof(IDefaultHandler<,>),
        typeof(ICommandHandler<>), typeof(IQueryRequestBehavior<,>), typeof(IQueryHandlerBehavior<,,>),
        typeof(ICommandRequestBehavior<>), typeof(ICommandHandlerBehavior<,>)))
      .AsImplementedInterfaces()
      .WithScopedLifetime());

    return services;
  }
}