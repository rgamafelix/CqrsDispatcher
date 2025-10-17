namespace RGamaFelix.CqrsDispatcher.Test.TestRequest;

public record DerivedCommandCommandRequest(string StrValue, int IntValue)
  : BaseCommandCommandRequest(StrValue, IntValue);