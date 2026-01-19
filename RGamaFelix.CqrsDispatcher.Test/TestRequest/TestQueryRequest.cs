using RGamaFelix.CqrsDispatcher.Query;

namespace RGamaFelix.CqrsDispatcher.Test.TestRequest;

public record TestQueryRequest : IQueryRequest<TestQueryResponse>;