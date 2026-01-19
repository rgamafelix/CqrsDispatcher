using RGamaFelix.CqrsDispatcher.Command.Handler;

namespace RGamaFelix.CqrsDispatcher.Command.Pipeline.Handler;

/// <summary>
///   Serves as a base class for defining extensions to command handlers in a CQRS dispatcher.
///   This class provides mechanisms to execute custom logic before and/or after the execution of a command handler,
///   as well as the ability to define execution order and filtering criteria.
/// </summary>
/// <typeparam name="THandler">
///   The type of the command handler that this extension applies to. Must implement
///   <see cref="ICommandHandler{TRequest}" />.
/// </typeparam>
/// <typeparam name="TRequest">
///   The type of the command request that this extension processes. Must implement <see cref="ICommandRequest" />.
/// </typeparam>
public abstract class CommandHandlerExtensionBase<THandler, TRequest> : ICommandHandlerExtension<THandler, TRequest>
  where THandler : ICommandHandler<TRequest> where TRequest : ICommandRequest
{
  /// <inheritdoc />
  public virtual int? Order { get; } = 0;

  /// <inheritdoc />
  public abstract Task Handle(TRequest request, THandler handler, Func<TRequest, CancellationToken, Task> next,
    CancellationToken cancellationToken);

  /// <inheritdoc />
  public virtual bool ShouldRun(TRequest request)
  {
    return true;
  }
}
