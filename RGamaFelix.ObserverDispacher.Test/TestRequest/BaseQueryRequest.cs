using RGamaFelix.CqrsDispatcher.Query;

namespace RGamaFelix.ObserverDispacher.Test.TestRequest;

public record BaseQueryRequest(string StrValue, int IntValue) : IQueryRequest<TestQueryResponse>;
