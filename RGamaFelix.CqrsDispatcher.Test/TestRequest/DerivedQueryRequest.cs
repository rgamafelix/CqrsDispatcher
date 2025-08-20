namespace RGamaFelix.CqrsDispatcher.Test.TestRequest;

public record DerivedQueryRequest(string StrValue, int IntValue) : BaseQueryRequest(StrValue, IntValue);