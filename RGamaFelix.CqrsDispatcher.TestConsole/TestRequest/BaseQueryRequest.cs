using RGamaFelix.CqrsDispatcher.Query;

namespace RGamaFelix.CqrsDispatcher.TestConsole.TestRequest;

public record BaseQueryRequest(string StrValue, int IntValue) : IQueryRequest<TestQueryResponse>;
