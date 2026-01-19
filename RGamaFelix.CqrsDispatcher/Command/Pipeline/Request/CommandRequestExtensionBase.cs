namespace RGamaFelix.CqrsDispatcher.Command.Pipeline.Request;

/// <summary>
///   Serves as a base class for implementing command request extensions, providing a framework for pre-processing,
///   post-processing, or modifying a command request. This class enables consistent implementation of behaviors
///   for handling command requests within the application.
/// </summary>
/// <typeparam name="TRequest">
///   The type of the command request to be handled. Must implement <see cref="ICommandRequest" />.
/// </typeparam>
public abstract class CommandRequestExtensionBase<TRequest> : ICommandRequestExtension<TRequest>
  where TRequest : ICommandRequest
{
  /// <inheritdoc />
  public virtual int? Order { get; } = 0;

  /// <inheritdoc />
  public abstract Task Handle(TRequest request, Func<TRequest, CancellationToken, Task> next,
    CancellationToken cancellationToken);

  /// <inheritdoc />
  public virtual bool ShouldRun(TRequest request)
  {
    return true;
  }
}