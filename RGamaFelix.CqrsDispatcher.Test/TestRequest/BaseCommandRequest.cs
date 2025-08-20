using RGamaFelix.CqrsDispatcher.Command;

namespace RGamaFelix.CqrsDispatcher.Test.TestRequest;

public record BaseCommandRequest(string StrValue, int IntValue) : ICommandRequest;