namespace RGamaFelix.CqrsDispatcher.Exceptions;

/// <summary>
///   Exception thrown when more than one handler is registered for a command type.
///   Commands require exactly one handler; use <see cref="IEvent" /> for fan-out scenarios.
/// </summary>
/// <typeparam name="TRequest">The command type for which multiple handlers were found.</typeparam>
public class MultipleCommandHandlersRegisteredException<TRequest> : Exception where TRequest : IRequestBase
{
  private const string DefaultErrorMessage =
    "Multiple handlers registered for command type {0}. Commands require exactly one handler. Use IEvent for fan-out scenarios.";

  public MultipleCommandHandlersRegisteredException() : base(string.Format(DefaultErrorMessage, typeof(TRequest)))
  {
  }

  public MultipleCommandHandlersRegisteredException(string message) : base(message)
  {
  }

  public MultipleCommandHandlersRegisteredException(string message, Exception innerException)
    : base(message, innerException)
  {
  }
}
