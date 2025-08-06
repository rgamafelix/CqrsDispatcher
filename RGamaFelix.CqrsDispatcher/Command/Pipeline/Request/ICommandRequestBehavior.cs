namespace RGamaFelix.CqrsDispatcher.Command.Pipeline.Request;

/// <summary>
///   Defines the behavior pipeline for handling command requests. Command request behaviors allow for the addition
///   of operations that should occur before or after the execution of a command handler, such as validation, logging, or
///   modifying the request.
/// </summary>
/// <typeparam name="TRequest">The type of the command request. Must implement <see cref="ICommandRequest" />.</typeparam>
public interface ICommandRequestBehavior<TRequest>
  where TRequest : ICommandRequest
{
  /// <summary>
  ///   Represents the execution order of the behavior within the command request pipeline. Specifies the order in
  ///   which command request behaviors should be executed. Lower values indicate higher precedence (executed earlier).
  /// </summary>
  /// <remarks>
  ///   If the value is <see langword="null" />, the behavior's order is undefined and may depend on other
  ///   configurations or runtime implementation details.
  /// </remarks>
  int? Order { get; }

  /// <summary>
  ///   Handles the execution of a command request within the behavior pipeline. Performs operations either before or
  ///   after the command handler itself.
  /// </summary>
  /// <param name="request">The command request being passed through the pipeline.</param>
  /// <param name="next">The next delegate in the pipeline to handle the request.</param>
  /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
  /// <returns>A task representing the asynchronous execution of the pipeline.</returns>
  Task Handle(TRequest request, Func<TRequest, CancellationToken, Task> next, CancellationToken cancellationToken);

  /// <summary>Determines whether the current behavior should be executed for the given command request.</summary>
  /// <param name="request">The command request to evaluate.</param>
  /// <returns>A boolean value indicating whether the behavior should run for the specified request.</returns>
  bool ShouldRun(TRequest request);
}