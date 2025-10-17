using RGamaFelix.RequestDispatcher.Query;

namespace RGamaFelix.RequestDispatcher.Test.TestRequest;

public record SelectableRequest(int IntValue) : IRequest<TestQueryResponse>;