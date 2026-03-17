namespace RGamaFelix.CqrsDispatcher.Authorization;

/// <summary>
///   Represents an attribute used to define authorization requirements for a target class.
///   This attribute is typically applied to handler classes to specify the authorization policy
///   required for execution. The policy is defined as a string that corresponds to authorization
///   rules in the underlying security model.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class HandlerAuthorizationAttribute : Attribute
{
  /// <summary>
  ///   Represents an attribute used to declare authorization requirements for a class.
  ///   This attribute is applied to classes to enforce authorization policies by specifying
  ///   the necessary claims or policies required for access.
  /// </summary>
  public HandlerAuthorizationAttribute(string policy)
  {
    Policy = policy;
  }

  /// <summary>
  ///   Gets the name of the authorization policy required for the associated handler.
  ///   This policy name is used to enforce specific authorization rules when the handler is executed,
  ///   ensuring that the appropriate security permissions are in place.
  /// </summary>
  public string Policy { get; }
}
