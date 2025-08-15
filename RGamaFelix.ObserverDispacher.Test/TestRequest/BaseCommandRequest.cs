using RGamaFelix.CqrsDispatcher.Command;

namespace RGamaFelix.ObserverDispacher.Test.TestRequest;

public record BaseCommandRequest(string StrValue, int IntValue) : ICommandRequest;
