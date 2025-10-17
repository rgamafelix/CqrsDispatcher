namespace RGamaFelix.CqrsDispatcher.Command.Handler;

/// <summary>
///   Defines a contract for handling commands of a specific type. Implementations of this interface provide the
///   logic to process incoming command requests.
/// </summary>
/// <typeparam name="TRequest">
///   The type of the command request that this handler processes. Must implement
///   <see cref="ICommandRequest" />.
/// </typeparam>
public interface ICommandHandler<in TRequest> where TRequest : ICommandRequest
{
  /// <summary>
  /// Handles an incoming command request of the specified type.
  /// The method encapsulates the logic needed to process the request
  /// and perform the corresponding operations.
  /// </summary>
  /// <param name="request">The command request instance that needs to be processed.</param>
  /// <param name="cancellationToken">A token to observe while waiting for the task to complete, enabling cancellation of the operation.</param>
  /// <returns>A task that represents the asynchronous operation of handling the command.</returns>
  Task Handle(TRequest request, CancellationToken cancellationToken);
}
