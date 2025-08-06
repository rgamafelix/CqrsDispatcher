namespace RGamaFelix.CqrsDispatcher;

/// <summary>Marker interface representing a request in the CQRS pattern.</summary>
/// <remarks>
///   The <see cref="IRequest" /> interface serves as a base type for all command and query requests in the CQRS
///   architecture. It is designed to establish a common contract for handling requests, enabling decoupling between the
///   request definitions and their handlers.
/// </remarks>
public interface IRequest;