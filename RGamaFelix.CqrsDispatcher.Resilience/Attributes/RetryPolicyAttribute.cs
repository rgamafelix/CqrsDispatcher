namespace RGamaFelix.CqrsDispatcher.Resilience;

/// <summary>
///   Declares a retry policy on an event handler class. The handler will be retried up to
///   <see cref="MaxAttempts" /> times on failure before a <see cref="EventHandlerRetryExhaustedException" /> is thrown.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class RetryPolicyAttribute : Attribute
{
  public RetryPolicyAttribute(int maxAttempts)
  {
    if (maxAttempts < 1)
    {
      throw new ArgumentOutOfRangeException(nameof(maxAttempts), "MaxAttempts must be at least 1.");
    }

    MaxAttempts = maxAttempts;
  }

  /// <summary>Maximum number of attempts, including the first. Must be ≥ 1.</summary>
  public int MaxAttempts { get; }

  /// <summary>Milliseconds to wait between attempts. Defaults to 0 (no delay).</summary>
  public int DelayMilliseconds { get; init; } = 0;
}
