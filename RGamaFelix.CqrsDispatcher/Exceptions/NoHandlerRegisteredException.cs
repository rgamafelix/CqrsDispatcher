namespace RGamaFelix.CqrsDispatcher.Exceptions;

/// <summary>Exception that is thrown when no handler is registered for a specific request type.</summary>
/// <typeparam name="TRequest">
///   The type of the request for which the exception is thrown. Typically implements the
///   <see cref="IRequest" /> interface.
/// </typeparam>
public class NoHandlerRegisteredException<TRequest> : Exception
  where TRequest : IRequest
{
  /// <summary>
  ///   Represents an exception that is thrown when no handler is registered for a given request type that implements
  ///   the <see cref="IRequest" /> interface.
  /// </summary>
  /// <typeparam name="TRequest">The type of the request for which the handler is missing.</typeparam>
  /// <remarks>
  ///   This exception is typically used in scenarios where the request/handler pattern is utilized, and it indicates
  ///   that a request was not properly configured or registered with a handler in the application setup.
  /// </remarks>
  public NoHandlerRegisteredException() : base(string.Format(DefaultErrorMessage, typeof(TRequest)))
  {
  }

  /// <summary>
  ///   Represents an exception that occurs when no handler is registered for a specific request of type
  ///   <see cref="IRequest" />.
  /// </summary>
  /// <typeparam name="TRequest">The type of the request for which the handler is missing.</typeparam>
  /// <remarks>
  ///   This exception is used in request/handler patterns to indicate that a request does not have an associated
  ///   handler configured.
  /// </remarks>
  public NoHandlerRegisteredException(string message) : base(message)
  {
  }

  /// <summary>Exception that is thrown when no handler is registered for a specific request type.</summary>
  /// <typeparam name="TRequest">
  ///   The type of the request for which the exception is thrown. Typically implements the
  ///   <see cref="IRequest" /> interface.
  /// </typeparam>
  /// <remarks>
  ///   This exception is used to indicate that a request in the request/handler pattern does not have a corresponding
  ///   handler registered. It helps identify misconfigurations or missing handler registrations during application setup.
  /// </remarks>
  public NoHandlerRegisteredException(string message, Exception innerException) : base(message, innerException)
  {
  }

  private const string DefaultErrorMessage = "No handler registered for request type {0}";
}