namespace RGamaFelix.CqrsDispatcher.Exceptions;

/// <summary>Represents an exception that is thrown when no handler selector is found for a given request type.</summary>
/// <typeparam name="TRequest">The type of the request that caused the exception. Must implement <see cref="IRequest" />.</typeparam>
public class NoHandlerSelectorFoundException<TRequest> : Exception
  where TRequest : IRequest
{
  /// <summary>Exception thrown when no handler selector is found for a given request type.</summary>
  /// <typeparam name="TRequest">
  ///   The type of the request that caused the exception. This type must implement the
  ///   <see cref="IRequest" /> interface.
  /// </typeparam>
  public NoHandlerSelectorFoundException() : base(string.Format(ErrorMessageFormat, typeof(TRequest)))
  {
  }

  /// <summary>Exception thrown when no handler selector is found for a given request type.</summary>
  /// <typeparam name="TRequest">
  ///   The type of the request that caused the exception. This type must implement the
  ///   <see cref="IRequest" /> interface.
  /// </typeparam>
  public NoHandlerSelectorFoundException(string message) : base(message)
  {
  }

  /// <summary>Exception thrown when no handler selector is found for a specified request type.</summary>
  /// <typeparam name="TRequest">
  ///   Specifies the type of the request that triggered the exception. This type must implement the
  ///   <see cref="IRequest" /> interface.
  /// </typeparam>
  public NoHandlerSelectorFoundException(string message, Exception innerException) : base(message, innerException)
  {
  }

  private const string ErrorMessageFormat = "No handler selected for request type {0}";
}