using RGamaFelix.CqrsDispatcher.Query;

namespace RGamaFelix.CqrsDispatcher.Test.TestRequest;

public record AlternateTestQueryRequest : IQueryRequest<TestQueryResponse>;