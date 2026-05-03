using Microsoft.Extensions.DependencyInjection;
using RGamaFelix.CqrsDispatcher.Event.Pipeline.Handler;

namespace RGamaFelix.CqrsDispatcher.Resilience.Configuration;

/// <summary>
///   Extension methods for registering the retry resilience extension in the CQRS pipeline.
/// </summary>
public static class Setup
{
  extension(IServiceCollection services)
  {
    /// <summary>
    ///   Registers <see cref="RetryEventHandlerExtension{THandler,TEvent}" /> as a scoped open-generic
    ///   <see cref="IEventHandlerExtension{THandler,TEvent}" />. Handlers decorated with
    ///   <see cref="RetryPolicyAttribute" /> will automatically be retried on failure.
    /// </summary>
    public IServiceCollection AddRetryExtensions()
    {
      services.AddScoped(typeof(IEventHandlerExtension<,>), typeof(RetryEventHandlerExtension<,>));

      return services;
    }
  }
}
