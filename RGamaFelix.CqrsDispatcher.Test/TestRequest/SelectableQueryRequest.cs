using RGamaFelix.CqrsDispatcher.Query;

namespace RGamaFelix.CqrsDispatcher.Test.TestRequest;

public record SelectableQueryRequest(int IntValue) : IQueryRequest<TestQueryResponse>;