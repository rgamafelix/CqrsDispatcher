using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RGamaFelix.CqrsDispatcher.Test;

public class TestHelper
{
  internal static IServiceCollection CreateCleanServices()
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
}