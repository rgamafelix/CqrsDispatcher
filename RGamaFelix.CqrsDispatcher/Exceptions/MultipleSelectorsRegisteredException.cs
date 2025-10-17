namespace RGamaFelix.CqrsDispatcher.Exceptions;

/// <summary>Exception that is thrown when multiple selector instances are registered for a specific request type.</summary>
/// <typeparam name="TRequest">The type of the request for which the multiple selectors are registered.</typeparam>
public class MultipleSelectorsRegisteredException<TRequest> : Exception where TRequest : IRequestBase
{
  private const string DefaultErrorMessage = "Multiple selectors found for request type {0}";

  /// <summary>Exception that is thrown when multiple selector instances are registered for a given request type.</summary>
  /// <typeparam name="TRequest">The type of the request for which multiple selectors are registered.</typeparam>
  public MultipleSelectorsRegisteredException() : base(string.Format(DefaultErrorMessage, typeof(TRequest)))
  {
  }

  /// <summary>Exception that is thrown when multiple selector instances are registered for a specific request type.</summary>
  /// <typeparam name="TRequest">The type of the request for which multiple selectors are registered.</typeparam>
  public MultipleSelectorsRegisteredException(string message) : base(message)
  {
  }

  /// <summary>Exception that is thrown when multiple selector instances are registered for a specified request type.</summary>
  /// <typeparam name="TRequest">The type of the request for which multiple selectors are registered.</typeparam>
  public MultipleSelectorsRegisteredException(string message, Exception innerException) : base(message, innerException)
  {
  }
}
