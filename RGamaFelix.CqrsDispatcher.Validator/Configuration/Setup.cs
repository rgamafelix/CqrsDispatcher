using Microsoft.Extensions.DependencyInjection;
using RGamaFelix.CqrsDispatcher.Command.Extension.Request;
using RGamaFelix.CqrsDispatcher.Query.Extension.Request;

namespace RGamaFelix.CqrsDispatcher.Validator.Configuration;

public static class Setup
{
  public static IServiceCollection RegisterCqrsDispatcherValidator(this IServiceCollection services)
  {
    services.AddScoped(typeof(ICommandRequestExtension<>), typeof(CommandRequestValidator<>));
    services.AddScoped(typeof(IQueryRequestExtension<,>), typeof(QueryRequestValidator<,>));

    return services;
  }
}