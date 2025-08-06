using RGamaFelix.CqrsDispatcher.Query;

namespace RGamaFelix.CqrsDispatcher.TestConsole.TestRequest;

public record SelectableQueryRequest(int IntValue) : IQueryRequest<TestQueryResponse>;