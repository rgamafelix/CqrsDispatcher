using RGamaFelix.CqrsDispatcher.Command;

namespace RGamaFelix.CqrsDispatcher.TestConsole.TestRequest;

public record BaseCommandRequest(string StrValue, int IntValue) : ICommandRequest;
