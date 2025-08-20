using Microsoft.Extensions.DependencyInjection;
using RGamaFelix.CqrsDispatcher.Command.Pipeline.Request;
using RGamaFelix.CqrsDispatcher.Query.Pipeline.Request;

namespace RGamaFelix.CqrsDispatcher.Validator.Config;

public static class CqrsDispatcherValidatorConfiguration
{
  public static IServiceCollection RegisterCqrsDispatcherValidator(this IServiceCollection services)
  {
    services.AddScoped(typeof(ICommandRequestBehavior<>), typeof(CommandRequestValidator<>));
    services.AddScoped(typeof(IQueryRequestBehavior<,>), typeof(QueryRequestValidator<,>));

    return services;
  }
}