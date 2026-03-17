using NSubstitute;
using RGamaFelix.CqrsDispatcher.Command.Handler;
using RGamaFelix.CqrsDispatcher.Command.Pipeline.Handler;
using RGamaFelix.CqrsDispatcher.Command.Pipeline.Request;
using RGamaFelix.CqrsDispatcher.Query;
using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Query.Pipeline.Handler;
using RGamaFelix.CqrsDispatcher.Query.Pipeline.Request;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test;

public class ExtensionBaseDefaultsTests
{
  private sealed class ConcreteCommandRequestExtension : CommandRequestExtensionBase<TestCommandRequest>
  {
    public override Task Handle(TestCommandRequest request, Func<TestCommandRequest, CancellationToken, Task> next,
      CancellationToken cancellationToken) => next(request, cancellationToken);
  }

  private sealed class ConcreteCommandHandlerExtension
    : CommandHandlerExtensionBase<ICommandHandler<TestCommandRequest>, TestCommandRequest>
  {
    public override Task Handle(TestCommandRequest request, ICommandHandler<TestCommandRequest> handler,
      Func<TestCommandRequest, CancellationToken, Task> next, CancellationToken cancellationToken)
      => next(request, cancellationToken);
  }

  private sealed class ConcreteQueryRequestExtension
    : QueryRequestExtensionBase<TestQueryRequest, TestQueryResponse>
  {
    public override Task<TestQueryResponse> Handle(TestQueryRequest request,
      Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>> next, CancellationToken cancellationToken)
      => next(request, cancellationToken);
  }

  private sealed class ConcreteQueryHandlerExtension
    : QueryHandlerExtensionBase<IQueryHandler<TestQueryRequest, TestQueryResponse>, TestQueryRequest, TestQueryResponse>
  {
    public override Task<TestQueryResponse> Handle(TestQueryRequest request,
      IQueryHandler<TestQueryRequest, TestQueryResponse> handler,
      Func<TestQueryRequest, CancellationToken, Task<TestQueryResponse>> next, CancellationToken cancellationToken)
      => next(request, cancellationToken);
  }

  [Fact]
  public void CommandRequestExtensionBaseShouldHaveDefaultOrderOfZero()
  {
    var sut = new ConcreteCommandRequestExtension();
    Assert.Equal(0, sut.Order);
  }

  [Fact]
  public void CommandRequestExtensionBaseShouldReturnTrueForShouldRun()
  {
    var sut = new ConcreteCommandRequestExtension();
    Assert.True(sut.ShouldRun(new TestCommandRequest()));
  }

  [Fact]
  public void CommandHandlerExtensionBaseShouldHaveDefaultOrderOfZero()
  {
    var sut = new ConcreteCommandHandlerExtension();
    Assert.Equal(0, sut.Order);
  }

  [Fact]
  public void CommandHandlerExtensionBaseShouldReturnTrueForShouldRun()
  {
    var sut = new ConcreteCommandHandlerExtension();
    Assert.True(sut.ShouldRun(new TestCommandRequest()));
  }

  [Fact]
  public void QueryRequestExtensionBaseShouldHaveDefaultOrderOfZero()
  {
    var sut = new ConcreteQueryRequestExtension();
    Assert.Equal(0, sut.Order);
  }

  [Fact]
  public void QueryRequestExtensionBaseShouldReturnTrueForShouldRun()
  {
    var sut = new ConcreteQueryRequestExtension();
    Assert.True(sut.ShouldRun(new TestQueryRequest()));
  }

  [Fact]
  public void QueryHandlerExtensionBaseShouldHaveDefaultOrderOfZero()
  {
    var sut = new ConcreteQueryHandlerExtension();
    Assert.Equal(0, sut.Order);
  }

  [Fact]
  public void QueryHandlerExtensionBaseShouldReturnTrueForShouldRun()
  {
    var sut = new ConcreteQueryHandlerExtension();
    Assert.True(sut.ShouldRun(new TestQueryRequest()));
  }
}
