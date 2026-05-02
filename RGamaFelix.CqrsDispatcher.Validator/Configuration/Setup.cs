using Microsoft.Extensions.DependencyInjection;
using RGamaFelix.CqrsDispatcher.Command.Pipeline.Request;
using RGamaFelix.CqrsDispatcher.Query.Pipeline.Request;

namespace RGamaFelix.CqrsDispatcher.Validator.Configuration;

/// <summary>
///   Provides extension methods for configuring and registering
///   validators for CQRS request pipelines within a DI container.
/// </summary>
public static class Setup
{
  /// <summary>
  ///   Registers validators for CQRS request pipelines in the dependency injection (DI) container.
  /// </summary>
  /// <param name="services">
  ///   The instance of <see cref="IServiceCollection" /> used to register services in the DI container.
  /// </param>
  /// <returns>
  ///   The modified instance of <see cref="IServiceCollection" /> with the CQRS validators registered.
  /// </returns>
  public static IServiceCollection RegisterCqrsDispatcherValidator(this IServiceCollection services)
  {
    services.AddScoped(typeof(ICommandRequestExtension<>), typeof(CommandRequestValidator<>));
    services.AddScoped(typeof(IQueryRequestExtension<,>), typeof(QueryRequestValidator<,>));

    return services;
  }
}