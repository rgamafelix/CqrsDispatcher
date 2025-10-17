using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using RGamaFelix.CqrsDispatcher.Command.Extension.Handler;
using RGamaFelix.CqrsDispatcher.Command.Extension.Request;
using RGamaFelix.CqrsDispatcher.Command.Handler;
using RGamaFelix.CqrsDispatcher.Query.Extension.Handler;
using RGamaFelix.CqrsDispatcher.Query.Extension.Request;
using RGamaFelix.CqrsDispatcher.Query.Handler;

namespace RGamaFelix.CqrsDispatcher.Configuration;

/// <summary>
///   Provides extension methods for configuring and registering components related to the CQRS Dispatcher
///   framework.
/// </summary>
public static class Setup
{
  /// <summary>Adds the CQRS Dispatcher Framework to the service collection, including core mediator configuration.</summary>
  /// <param name="services">The service collection to which the CQRS Dispatcher Framework will be added.</param>
  /// <returns>The updated service collection with the CQRS Dispatcher Framework registered.</returns>
  public static IServiceCollection AddCqrsDispatcherFramework(this IServiceCollection services)
  {
    // Core mediator
    services.AddSingleton<Dispatcher>();

    return services;
  }

  /// <summary>
  ///   Registers the CQRS Dispatcher components, including handlers, request extensions, and handler extensions, from
  ///   the specified assemblies into the service collection.
  /// </summary>
  /// <param name="services">The service collection to which the CQRS Dispatcher components will be registered.</param>
  /// <param name="assemblies">The assemblies to scan for CQRS Dispatcher components.</param>
  /// <returns>The updated service collection with the CQRS Dispatcher components registered.</returns>
  public static IServiceCollection RegisterCqrsDispatcherComponents(this IServiceCollection services,
    params Assembly[] assemblies)
  {
    services.Scan(scan => scan.FromAssemblies(assemblies)
      .AddClasses(classes => classes.AssignableToAny(typeof(IQueryHandler<,>), typeof(ICommandHandler<>),
        typeof(IQueryRequestExtension<,>), typeof(IQueryHandlerExtension<,,>), typeof(ICommandRequestExtension<>),
        typeof(ICommandHandlerExtension<,>)))
      .AsImplementedInterfaces()
      .WithScopedLifetime());

    return services;
  }
}
