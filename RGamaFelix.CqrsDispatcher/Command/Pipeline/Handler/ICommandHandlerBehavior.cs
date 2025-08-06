using RGamaFelix.CqrsDispatcher.Command.Handler;

namespace RGamaFelix.CqrsDispatcher.Command.Pipeline.Handler;

/// <summary>
///   Represents a behavior pipeline that can be executed around command handlers. This interface allows for
///   additional logic to be injected before and/or after the execution of a command handler.
/// </summary>
/// <typeparam name="THandler">
///   The type of the command handler that this behavior is applied to. Must implement
///   <see cref="ICommandHandler{TRequest}" />.
/// </typeparam>
/// <typeparam name="TRequest">
///   The type of the command request that this behavior processes. Must implement
///   <see cref="ICommandRequest" />.
/// </typeparam>
public interface ICommandHandlerBehavior<THandler, TRequest>
  where THandler : ICommandHandler<TRequest> where TRequest : ICommandRequest
{
  /// <summary>
  ///   Gets the order in which the behavior pipeline is executed. Behaviors with lower values for the order are
  ///   executed earlier, while behaviors with higher values are executed later. Provides a mechanism to control the sequence
  ///   of execution among multiple behaviors within the pipeline.
  /// </summary>
  /// <remarks>
  ///   The value is nullable to allow for scenarios where the order is not explicitly set or not applicable. A
  ///   default value, if any, should be assigned by implementations to maintain a predictable execution flow.
  /// </remarks>
  int? Order { get; }

  /// <summary>
  ///   Processes the specified command request using the provided handler and executes the next behavior in the
  ///   pipeline.
  /// </summary>
  /// <param name="request">The command request to be processed.</param>
  /// <param name="handler">The command handler responsible for processing the command request.</param>
  /// <param name="next">The next delegate in the handling pipeline to be executed after the current handling logic.</param>
  /// <param name="cancellationToken">A token that can be used to monitor cancellation requests.</param>
  /// <returns>A task that represents the asynchronous operation of handling the command request.</returns>
  Task Handle(TRequest request, THandler handler, Func<TRequest, CancellationToken, Task> next,
    CancellationToken cancellationToken);

  /// <summary>Determines whether the behavior pipeline should execute for the provided command request.</summary>
  /// <param name="request">The command request to evaluate.</param>
  /// <returns><c>true</c> if the behavior pipeline should execute for the provided command request; otherwise, <c>false</c>.</returns>
  bool ShouldRun(TRequest request);
}