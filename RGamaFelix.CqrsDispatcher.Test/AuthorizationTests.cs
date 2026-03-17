using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using RGamaFelix.CqrsDispatcher.Authorization;
using RGamaFelix.CqrsDispatcher.Command.Handler;
using RGamaFelix.CqrsDispatcher.Command.Pipeline.Handler;
using RGamaFelix.CqrsDispatcher.Query;
using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Query.Pipeline.Handler;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;

namespace RGamaFelix.CqrsDispatcher.Test;

public class AuthorizationTests
{
  private const string TestPolicy = "TestPolicy";

  [HandlerAuthorizationAttribute(TestPolicy)]
  private sealed class AuthorizedCommandHandler : ICommandHandler<TestCommandRequest>
  {
    public Task Handle(TestCommandRequest request, CancellationToken cancellationToken) => Task.CompletedTask;
  }

  private sealed class UnauthorizedCommandHandler : ICommandHandler<TestCommandRequest>
  {
    public Task Handle(TestCommandRequest request, CancellationToken cancellationToken) => Task.CompletedTask;
  }

  [HandlerAuthorizationAttribute(TestPolicy)]
  private sealed class AuthorizedQueryHandler : IQueryHandler<TestQueryRequest, TestQueryResponse>
  {
#pragma warning disable CS0618
    public Task<TestQueryResponse> HandleAsync(TestQueryRequest request, CancellationToken cancellationToken)
#pragma warning restore CS0618
      => Task.FromResult(new TestQueryResponse());
  }

  private sealed class UnauthorizedQueryHandler : IQueryHandler<TestQueryRequest, TestQueryResponse>
  {
#pragma warning disable CS0618
    public Task<TestQueryResponse> HandleAsync(TestQueryRequest request, CancellationToken cancellationToken)
#pragma warning restore CS0618
      => Task.FromResult(new TestQueryResponse());
  }

  private static IHttpContextAccessor BuildHttpContextAccessor(ClaimsPrincipal? user)
  {
    var accessor = Substitute.For<IHttpContextAccessor>();

    if (user is not null)
    {
      var httpContext = Substitute.For<HttpContext>();
      httpContext.User.Returns(user);
      accessor.HttpContext.Returns(httpContext);
    }
    else
    {
      accessor.HttpContext.Returns((HttpContext?)null);
    }

    return accessor;
  }

  [Fact]
  public void CommandHandlerExtensionShouldSkipWhenHandlerHasNoAuthorizationAttribute()
  {
    // Arrange
    var accessor = BuildHttpContextAccessor(null);
    var authService = Substitute.For<IAuthorizationService>();

    var sut = new AuthorizationCommandHandlerExtension<UnauthorizedCommandHandler, TestCommandRequest>(
      accessor, authService,
      NullLogger<ICommandHandlerExtension<UnauthorizedCommandHandler, TestCommandRequest>>.Instance);

    // Act & Assert — ShouldRun returns false, no exception even with null context
    Assert.False(sut.ShouldRun(new TestCommandRequest()));
  }

  [Fact]
  public void CommandHandlerExtensionShouldRunWhenHandlerHasAuthorizationAttribute()
  {
    // Arrange
    var accessor = BuildHttpContextAccessor(null);
    var authService = Substitute.For<IAuthorizationService>();

    var sut = new AuthorizationCommandHandlerExtension<AuthorizedCommandHandler, TestCommandRequest>(
      accessor, authService,
      NullLogger<ICommandHandlerExtension<AuthorizedCommandHandler, TestCommandRequest>>.Instance);

    // Act & Assert
    Assert.True(sut.ShouldRun(new TestCommandRequest()));
  }

  [Fact]
  public async Task CommandHandlerExtensionShouldThrowWhenNoHttpContext()
  {
    // Arrange
    var accessor = BuildHttpContextAccessor(null);
    var authService = Substitute.For<IAuthorizationService>();

    var sut = new AuthorizationCommandHandlerExtension<AuthorizedCommandHandler, TestCommandRequest>(
      accessor, authService,
      NullLogger<ICommandHandlerExtension<AuthorizedCommandHandler, TestCommandRequest>>.Instance);

    // Act & Assert
    await Assert.ThrowsAsync<UnauthorizedRequestException>(
      () => sut.Handle(new TestCommandRequest(), new AuthorizedCommandHandler(),
        (_, _) => Task.CompletedTask, CancellationToken.None));
  }

  [Fact]
  public async Task CommandHandlerExtensionShouldThrowWhenPolicyFails()
  {
    // Arrange
    var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "user")], "test"));
    var accessor = BuildHttpContextAccessor(user);
    var authService = Substitute.For<IAuthorizationService>();
    authService.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object?>(), Arg.Any<string>())
      .Returns(AuthorizationResult.Failed());

    var sut = new AuthorizationCommandHandlerExtension<AuthorizedCommandHandler, TestCommandRequest>(
      accessor, authService,
      NullLogger<ICommandHandlerExtension<AuthorizedCommandHandler, TestCommandRequest>>.Instance);

    // Act & Assert
    await Assert.ThrowsAsync<UnauthorizedRequestException>(
      () => sut.Handle(new TestCommandRequest(), new AuthorizedCommandHandler(),
        (_, _) => Task.CompletedTask, CancellationToken.None));
  }

  [Fact]
  public async Task CommandHandlerExtensionShouldCallNextWhenPolicySucceeds()
  {
    // Arrange
    var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "user")], "test"));
    var accessor = BuildHttpContextAccessor(user);
    var authService = Substitute.For<IAuthorizationService>();
    authService.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object?>(), Arg.Any<string>())
      .Returns(AuthorizationResult.Success());

    var nextCalled = false;
    var sut = new AuthorizationCommandHandlerExtension<AuthorizedCommandHandler, TestCommandRequest>(
      accessor, authService,
      NullLogger<ICommandHandlerExtension<AuthorizedCommandHandler, TestCommandRequest>>.Instance);

    Task Next(TestCommandRequest _, CancellationToken __) { nextCalled = true; return Task.CompletedTask; }

    // Act
    await sut.Handle(new TestCommandRequest(), new AuthorizedCommandHandler(), Next, CancellationToken.None);

    // Assert
    Assert.True(nextCalled);
  }

  [Fact]
  public void QueryHandlerExtensionShouldSkipWhenHandlerHasNoAuthorizationAttribute()
  {
    // Arrange
    var accessor = BuildHttpContextAccessor(null);
    var authService = Substitute.For<IAuthorizationService>();

    var sut = new AuthorizationQueryHandlerExtension<UnauthorizedQueryHandler, TestQueryRequest, TestQueryResponse>(
      accessor, authService,
      NullLogger<IQueryHandlerExtension<UnauthorizedQueryHandler, TestQueryRequest, TestQueryResponse>>.Instance);

    // Act & Assert
    Assert.False(sut.ShouldRun(new TestQueryRequest()));
  }

  [Fact]
  public async Task QueryHandlerExtensionShouldThrowWhenNoHttpContext()
  {
    // Arrange
    var accessor = BuildHttpContextAccessor(null);
    var authService = Substitute.For<IAuthorizationService>();

    var sut = new AuthorizationQueryHandlerExtension<AuthorizedQueryHandler, TestQueryRequest, TestQueryResponse>(
      accessor, authService,
      NullLogger<IQueryHandlerExtension<AuthorizedQueryHandler, TestQueryRequest, TestQueryResponse>>.Instance);

    // Act & Assert
    await Assert.ThrowsAsync<UnauthorizedRequestException>(
      () => sut.Handle(new TestQueryRequest(), new AuthorizedQueryHandler(),
        (_, _) => Task.FromResult(new TestQueryResponse()), CancellationToken.None));
  }

  [Fact]
  public async Task QueryHandlerExtensionShouldCallNextWhenPolicySucceeds()
  {
    // Arrange
    var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "user")], "test"));
    var accessor = BuildHttpContextAccessor(user);
    var authService = Substitute.For<IAuthorizationService>();
    authService.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object?>(), Arg.Any<string>())
      .Returns(AuthorizationResult.Success());

    var nextCalled = false;
    var sut = new AuthorizationQueryHandlerExtension<AuthorizedQueryHandler, TestQueryRequest, TestQueryResponse>(
      accessor, authService,
      NullLogger<IQueryHandlerExtension<AuthorizedQueryHandler, TestQueryRequest, TestQueryResponse>>.Instance);

    Task<TestQueryResponse> Next(TestQueryRequest _, CancellationToken __)
    {
      nextCalled = true;
      return Task.FromResult(new TestQueryResponse());
    }

    // Act
    await sut.Handle(new TestQueryRequest(), new AuthorizedQueryHandler(), Next, CancellationToken.None);

    // Assert
    Assert.True(nextCalled);
  }
}
