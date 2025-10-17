namespace RGamaFelix.CqrsDispatcher.Command;

/// <summary>
///   Represents a command request that can be processed within the CQRS (Command Query Responsibility Segregation)
///   pattern. Implementing this interface indicates that the derived type is a command, which encapsulates the specific
///   action or process to be executed.
/// </summary>
/// <remarks>
///   Command requests signify a "write" intent within a system and are distinct from queries, which are focused on
///   data retrieval. This interface serves as a marker interface, potentially extended by concrete implementations to
///   include additional properties or methods specific to the respective command.
/// </remarks>
public interface ICommandRequest : IRequestBase;