using Microsoft.Extensions.DependencyInjection;
using RGamaFelix.CqrsDispatcher.Command.Handler;
using RGamaFelix.CqrsDispatcher.Config;
using RGamaFelix.CqrsDispatcher.Exceptions;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test;

public class CommandHandlerTest
{
  [Fact]
  public async Task DispatcherShouldThrowExceptionWhenNoCommandHandlerIsRegisteredTest()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    services.AddCqrsDispatcherFramework();
    var provider = services.BuildServiceProvider();
    var dispatcher = provider.GetRequiredService<Dispatcher>();

    // Act
    await Assert.ThrowsAsync<NoHandlerRegisteredException<BaseCommandRequest>>(async () =>
    {
      await dispatcher.Publish(new BaseCommandRequest("strValue", 1));
    });
  }

  // [Fact]
  // public void DispatcherShouldRunWhenMultiplesHandlersAreFound()
  // {
  //   // Arrange
  //   var services = TestHelper.CreateCleanServices();
  //   services.AddCqrsDispatcherFramework();
  //   services.AddScoped<ICommandHandler<BaseCommandRequest>, >();
  //   services.AddScoped<ICommandHandler<BaseCommandRequest>, handlerMock2>();
  //   var provider = services.BuildServiceProvider();
  //   var dispatcher = provider.GetRequiredService<Dispatcher>();
  //
  //
  // }
}
