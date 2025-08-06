namespace RGamaFelix.CqrsDispatcher.Exceptions;

/// <summary>Exception that is thrown when no handler selector is registered for a specific request type.</summary>
/// <typeparam name="TRequest">
///   The type of the request for which the exception is thrown. Must implement the
///   <see cref="IRequest" /> interface.
/// </typeparam>
public class NoHandlerSelectorRegisteredException<TRequest> : Exception
  where TRequest : IRequest
{
  /// <summary>
  ///   Initializes a new instance of the <see cref="NoHandlerSelectorRegisteredException{TRequest}" /> class with a
  ///   default error message.
  /// </summary>
  public NoHandlerSelectorRegisteredException() : base(string.Format(DefaultErrorMessage, typeof(TRequest)))
  {
  }

  /// <summary>
  ///   Initializes a new instance of the <see cref="NoHandlerSelectorRegisteredException{TRequest}" /> class with a
  ///   specified error message.
  /// </summary>
  /// <param name="message">The message that describes the error.</param>
  public NoHandlerSelectorRegisteredException(string message) : base(message)
  {
  }

  /// <summary>
  ///   Initializes a new instance of the <see cref="NoHandlerSelectorRegisteredException{TRequest}" /> class with a
  ///   specified error message and a reference to the inner exception that is the cause of this exception.
  /// </summary>
  /// <param name="message">The error message that explains the reason for the exception.</param>
  /// <param name="innerException">The exception that is the cause of the current exception.</param>
  public NoHandlerSelectorRegisteredException(string message, Exception innerException) : base(message, innerException)
  {
  }

  private const string DefaultErrorMessage = "No handler selector registered for request type {0}";
}