using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using RGamaFelix.CqrsDispatcher.Command.Handler;
using RGamaFelix.CqrsDispatcher.Command.Pipeline.Handler;
using RGamaFelix.CqrsDispatcher.Command.Pipeline.Request;
using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Query.Pipeline.Handler;
using RGamaFelix.CqrsDispatcher.Query.Pipeline.Request;

namespace RGamaFelix.CqrsDispatcher.Configuration;

/// <summary>
///   Provides extension methods for configuring and registering components related to the CQRS Dispatcher
///   framework.
/// </summary>
public static class Setup
{
  private static readonly HashSet<Type> _serviceTypeDefinitions =
  [
    typeof(IQueryHandler<,>),
    typeof(ICommandHandler<>),
    typeof(IQueryRequestExtension<,>),
    typeof(IQueryHandlerExtension<,,>),
    typeof(ICommandRequestExtension<>),
    typeof(ICommandHandlerExtension<,>)
  ];

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
    if (assemblies is null || assemblies.Length == 0)
    {
      return services;
    }

    foreach (var assembly in assemblies.Distinct())
    {
      foreach (var implementationType in assembly.DefinedTypes
                 .Where(t => t is { IsClass: true, IsAbstract: false, ContainsGenericParameters: false })
                 .Select(t => t.AsType()))
      {
        // Register only the matching implemented interfaces (like Scrutor's AsImplementedInterfaces()).
        var matchingServiceTypes = implementationType.GetInterfaces()
          .Where(i => i.IsGenericType)
          .Where(i => _serviceTypeDefinitions.Contains(i.GetGenericTypeDefinition()));

        foreach (var serviceType in matchingServiceTypes)
        {
          services.AddScoped(serviceType, implementationType);
        }
      }
    }

    return services;
  }
}