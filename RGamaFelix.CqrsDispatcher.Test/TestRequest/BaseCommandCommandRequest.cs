using RGamaFelix.CqrsDispatcher.Command;

namespace RGamaFelix.CqrsDispatcher.Test.TestRequest;

public record BaseCommandCommandRequest(string StrValue, int IntValue) : ICommandRequest;