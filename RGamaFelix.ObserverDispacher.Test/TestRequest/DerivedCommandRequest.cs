namespace RGamaFelix.ObserverDispacher.Test.TestRequest;

public record DerivedCommandRequest(string StrValue, int IntValue) : BaseCommandRequest(StrValue, IntValue);
