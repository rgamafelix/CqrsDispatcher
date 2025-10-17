using Microsoft.Extensions.DependencyInjection;
using RGamaFelix.CqrsDispatcher.Command.Handler;
using RGamaFelix.CqrsDispatcher.Configuration;
using RGamaFelix.CqrsDispatcher.Exceptions;
using RGamaFelix.CqrsDispatcher.Test.Handlers.Command;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test;

public class CommandHandlerTest
{
  [Fact]
  public async Task DispatcherShouldRunWhenMultiplesCommandHandlersAreFound()
  {
    // Arrange
    var baseHandlerCallCount = 0;
    var alternateHandlerCallCount = 0;
    var services = TestHelper.CreateCleanServices();
    services.AddCqrsDispatcherFramework();

    services.AddScoped<ICommandHandler<BaseCommandCommandRequest>>(_ =>
    {
      return new BaseCommandHandler(() => baseHandlerCallCount++);
    });

    services.AddScoped<ICommandHandler<BaseCommandCommandRequest>>(_ =>
    {
      return new AlternateCommandHandler(() => alternateHandlerCallCount++);
    });

    var provider = services.BuildServiceProvider();
    var dispatcher = provider.GetRequiredService<Dispatcher>();

    // Act
    await dispatcher.Publish(new BaseCommandCommandRequest("strValue", 1));

    // Assert
    Assert.Equal(1, baseHandlerCallCount);
    Assert.Equal(1, alternateHandlerCallCount);
  }

  [Fact]
  public async Task DispatcherShouldRunWhenSingleCommandHandlerIsFound()
  {
    // Arrange
    var baseHandlerCallCount = 0;
    var services = TestHelper.CreateCleanServices();
    services.AddCqrsDispatcherFramework();

    services.AddScoped<ICommandHandler<BaseCommandCommandRequest>>(_ =>
    {
      return new BaseCommandHandler(() => baseHandlerCallCount++);
    });

    var provider = services.BuildServiceProvider();
    var dispatcher = provider.GetRequiredService<Dispatcher>();
    // Act
    await dispatcher.Publish(new BaseCommandCommandRequest("strValue", 1));
    // Assert
    Assert.Equal(1, baseHandlerCallCount);
  }

  [Fact]
  public async Task DispatcherShouldThrowExceptionWhenNoCommandHandlerIsRegisteredTest()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    services.AddCqrsDispatcherFramework();
    var provider = services.BuildServiceProvider();
    var dispatcher = provider.GetRequiredService<Dispatcher>();

    // Act
    await Assert.ThrowsAsync<NoHandlerRegisteredException<BaseCommandCommandRequest>>(async () =>
    {
      await dispatcher.Publish(new BaseCommandCommandRequest("strValue", 1));
    });
  }
}