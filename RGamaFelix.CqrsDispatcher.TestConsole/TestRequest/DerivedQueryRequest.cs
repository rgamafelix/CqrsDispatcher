namespace RGamaFelix.CqrsDispatcher.TestConsole.TestRequest;

public record DerivedQueryRequest(string StrValue, int IntValue) : BaseQueryRequest(StrValue, IntValue);