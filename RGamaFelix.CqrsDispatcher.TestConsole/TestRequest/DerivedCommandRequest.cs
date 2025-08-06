namespace RGamaFelix.CqrsDispatcher.TestConsole.TestRequest;

public record DerivedCommandRequest(string StrValue, int IntValue) : BaseCommandRequest(StrValue, IntValue);
