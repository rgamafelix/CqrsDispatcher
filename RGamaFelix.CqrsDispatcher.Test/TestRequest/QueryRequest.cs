using RGamaFelix.CqrsDispatcher.Query;

namespace RGamaFelix.CqrsDispatcher.Test.TestRequest;

public record QueryRequest(string StrValue, int IntValue) : IQueryRequest<TestQueryResponse>;