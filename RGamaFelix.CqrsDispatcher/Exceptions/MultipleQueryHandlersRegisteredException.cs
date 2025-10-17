namespace RGamaFelix.CqrsDispatcher.Exceptions;

/// <summary>An exception that is thrown when multiple query handlers are found for a specific request type.</summary>
/// <typeparam name="TRequest">The type of the request for which multiple query handlers were found.</typeparam>
/// <remarks>
///   This exception is used in scenarios where the CQRS dispatcher detects more than one query handler for a single
///   request type, violating the assumption that there should only be one query handler per request.
/// </remarks>
public class MultipleQueryHandlersRegisteredException<TRequest> : Exception where TRequest : IRequestBase
{
  private const string DefaultErrorMessage = "Multiple query handlers found for request type {0}";

  /// <summary>Represents an exception that is thrown when multiple query handlers are selected for a specific request type.</summary>
  /// <typeparam name="TRequest">The type of the request for which the exception occurred.</typeparam>
  /// <remarks>
  ///   This exception is typically used in scenarios where the CQRS dispatcher detects more than one query handler
  ///   for a single request type. This violates the requirement that only one query handler should exist per request type.
  /// </remarks>
  public MultipleQueryHandlersRegisteredException() : base(string.Format(DefaultErrorMessage, typeof(TRequest)))
  {
  }

  /// <summary>Represents an exception that is thrown when multiple query handlers are selected for a specific request type.</summary>
  /// <typeparam name="TRequest">The type of the request for which the exception occurred.</typeparam>
  /// <remarks>
  ///   This exception is typically used in scenarios where the CQRS dispatcher detects more than one query handler
  ///   for a single request type. This violates the requirement that only one query handler should exist per request type.
  /// </remarks>
  public MultipleQueryHandlersRegisteredException(string message) : base(message)
  {
  }

  /// <summary>Represents an exception that is thrown when multiple query handlers are selected for a specific request type.</summary>
  /// <typeparam name="TRequest">The type of the request for which the exception occurred.</typeparam>
  /// <remarks>
  ///   This exception is typically used in scenarios where the CQRS dispatcher detects more than one query handler
  ///   for a specific request type. This situation violates the expectation that only one query handler should exist for
  ///   each request type.
  /// </remarks>
  public MultipleQueryHandlersRegisteredException(string message, Exception innerException) : base(message,
    innerException)
  {
  }
}
