using Microsoft.Extensions.Logging.Abstractions;
using RGamaFelix.CqrsDispatcher.Event;
using RGamaFelix.CqrsDispatcher.Event.Handler;
using RGamaFelix.CqrsDispatcher.Event.Pipeline.Handler;
using RGamaFelix.CqrsDispatcher.Resilience;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test;

public class RetryExtensionTests
{
  private static RetryEventHandlerExtension<THandler, TEvent> BuildSut<THandler, TEvent>()
    where THandler : IEventHandler<TEvent> where TEvent : class, IEvent
  {
    return new RetryEventHandlerExtension<THandler, TEvent>(
      NullLogger<IEventHandlerExtension<THandler, TEvent>>.Instance);
  }

  [Fact]
  public void ShouldRunReturnsTrueWhenHandlerHasRetryAttribute()
  {
    var sut = BuildSut<RetryableEventHandler, TestEvent>();

    Assert.True(sut.ShouldRun(new TestEvent()));
  }

  [Fact]
  public void ShouldRunReturnsFalseWhenHandlerHasNoRetryAttribute()
  {
    var sut = BuildSut<NonRetryableEventHandler, TestEvent>();

    Assert.False(sut.ShouldRun(new TestEvent()));
  }

  [Fact]
  public async Task ShouldCallNextOnceWhenHandlerSucceedsOnFirstAttempt()
  {
    var callCount = 0;
    var sut = BuildSut<RetryableEventHandler, TestEvent>();

    await sut.Handle(new TestEvent(), new RetryableEventHandler(), (_, _) =>
    {
      callCount++;

      return Task.CompletedTask;
    }, CancellationToken.None);

    Assert.Equal(1, callCount);
  }

  [Fact]
  public async Task ShouldRetryAndSucceedWhenFirstAttemptFails()
  {
    var callCount = 0;
    var sut = BuildSut<RetryableEventHandler, TestEvent>();

    await sut.Handle(new TestEvent(), new RetryableEventHandler(), (_, _) =>
    {
      if (++callCount == 1)
      {
        throw new InvalidOperationException("transient failure");
      }

      return Task.CompletedTask;
    }, CancellationToken.None);

    Assert.Equal(2, callCount);
  }

  [Fact]
  public async Task ShouldThrowRetryExhaustedExceptionWhenAllAttemptsFail()
  {
    var sut = BuildSut<RetryableEventHandler, TestEvent>();

    var ex = await Assert.ThrowsAsync<EventHandlerRetryExhaustedException>(() =>
      sut.Handle(new TestEvent(), new RetryableEventHandler(), (_, _) =>
        throw new InvalidOperationException("always fails"), CancellationToken.None));

    Assert.Equal(typeof(RetryableEventHandler), ex.HandlerType);
    Assert.Equal(3, ex.MaxAttempts);
    Assert.Equal(3, ex.InnerExceptions.Count);
  }

  [Fact]
  public async Task ShouldNotRetryOnCancellation()
  {
    var callCount = 0;
    var cts = new CancellationTokenSource();
    var sut = BuildSut<RetryableEventHandler, TestEvent>();

    await Assert.ThrowsAsync<OperationCanceledException>(() =>
      sut.Handle(new TestEvent(), new RetryableEventHandler(), (_, _) =>
      {
        callCount++;
        cts.Cancel();
        throw new OperationCanceledException(cts.Token);
      }, cts.Token));

    Assert.Equal(1, callCount);
  }

  [Fact]
  public async Task ShouldRespectMaxAttempts()
  {
    var callCount = 0;
    var sut = BuildSut<SingleAttemptEventHandler, TestEvent>();

    await Assert.ThrowsAsync<EventHandlerRetryExhaustedException>(() =>
      sut.Handle(new TestEvent(), new SingleAttemptEventHandler(), (_, _) =>
      {
        callCount++;
        throw new InvalidOperationException("always fails");
      }, CancellationToken.None));

    Assert.Equal(1, callCount);
  }

  [RetryPolicy(3)]
  private sealed class RetryableEventHandler : IEventHandler<TestEvent>
  {
    public Task Handle(TestEvent @event, CancellationToken cancellationToken) => Task.CompletedTask;
  }

  [RetryPolicy(1)]
  private sealed class SingleAttemptEventHandler : IEventHandler<TestEvent>
  {
    public Task Handle(TestEvent @event, CancellationToken cancellationToken) => Task.CompletedTask;
  }

  private sealed class NonRetryableEventHandler : IEventHandler<TestEvent>
  {
    public Task Handle(TestEvent @event, CancellationToken cancellationToken) => Task.CompletedTask;
  }
}
