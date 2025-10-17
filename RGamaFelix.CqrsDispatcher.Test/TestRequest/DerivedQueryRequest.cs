namespace RGamaFelix.CqrsDispatcher.Test.TestRequest;

public record DerivedQueryRequest(string StrValue, int IntValue) : QueryRequest(StrValue, IntValue);