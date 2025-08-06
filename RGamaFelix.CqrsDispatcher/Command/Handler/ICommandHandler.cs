namespace RGamaFelix.CqrsDispatcher.Command.Handler;

/// <summary>
///   Defines a contract for handling commands of a specific type. Implementations of this interface provide the
///   logic to process incoming command requests.
/// </summary>
/// <typeparam name="TRequest">
///   The type of the command request that this handler processes. Must implement
///   <see cref="ICommandRequest" />.
/// </typeparam>
public interface ICommandHandler<in TRequest>
  where TRequest : ICommandRequest
{
  Task Handle(TRequest request, CancellationToken cancellationToken);
}