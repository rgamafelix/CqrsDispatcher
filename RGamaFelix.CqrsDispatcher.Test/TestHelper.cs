using Microsoft.Extensions.DependencyInjection;
using RGamaFelix.CqrsDispatcher.Configuration;

namespace RGamaFelix.CqrsDispatcher.Test;

public class TestHelper
{
  internal static IServiceCollection CreateCleanServices()
  {
    var services = new ServiceCollection();
    services.AddCqrsDispatcherFramework();

    return services;
  }
}