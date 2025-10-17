namespace RGamaFelix.CqrsDispatcher;

/// <summary>Marker interface representing a request in the CQRS pattern.</summary>
/// <remarks>
///   This Type Should NOT be used directly. The <see cref="IRequestBase" /> interface serves as a base type for all
///   command and query requests in the CqrsDispatcher architecture. It is designed to establish a common contract for
///   handling requests, enabling decoupling between the request definitions and their handlers.
/// </remarks>
public interface IRequestBase;