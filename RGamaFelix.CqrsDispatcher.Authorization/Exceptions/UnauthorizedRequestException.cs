namespace RGamaFelix.CqrsDispatcher.Authorization;

/// <summary>
///   Represents an exception that is thrown when an unauthorized request is detected.
/// </summary>
/// <remarks>
///   This exception is specifically used to indicate that a request does not meet the necessary
///   authorization requirements, such as missing user context or failing an authorization policy.
///   It is typically utilized within middleware or extensions that handle command or query
///   authorization.
/// </remarks>
public class UnauthorizedRequestException : Exception
{
  /// <summary>
  ///   Represents an exception thrown when an unauthorized request is detected in the application.
  /// </summary>
  /// <remarks>
  ///   This exception signifies that a request does not meet the required authorization criteria,
  ///   such as a missing user context or failing an authorization policy. It is used to enforce
  ///   application security and is typically thrown when processing command or query requests
  ///   that require specific authorization checks.
  /// </remarks>
  public UnauthorizedRequestException(string message) : base(message)
  {
  }
}