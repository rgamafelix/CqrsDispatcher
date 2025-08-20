using RGamaFelix.CqrsDispatcher.Query;

namespace RGamaFelix.CqrsDispatcher.Test.TestRequest;

public record BaseQueryRequest(string StrValue, int IntValue) : IQueryRequest<TestQueryResponse>;