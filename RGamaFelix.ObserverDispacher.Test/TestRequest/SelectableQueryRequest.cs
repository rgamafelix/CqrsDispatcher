using RGamaFelix.CqrsDispatcher.Query;

namespace RGamaFelix.ObserverDispacher.Test.TestRequest;

public record SelectableQueryRequest(int IntValue) : IQueryRequest<TestQueryResponse>;