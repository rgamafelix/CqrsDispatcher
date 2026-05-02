using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RGamaFelix.CqrsDispatcher.Query;
using RGamaFelix.CqrsDispatcher.Query.Handler;
using RGamaFelix.CqrsDispatcher.Query.Pipeline.Handler;

namespace RGamaFelix.CqrsDispatcher.Authorization;

/// <summary>
///   A query handler extension that provides authorization checks for query handlers.
///   It intercepts the execution of a query handler to ensure that the user is authorized
///   to execute the specific query request.
/// </summary>
/// <typeparam name="THandler">
///   The type of the query handler being extended. Must implement <see cref="IQueryHandler{TRequest,TResponse}" />.
/// </typeparam>
/// <typeparam name="TRequest">
///   The type of the query request being processed. Must implement <see cref="IQueryRequest{TResponse}" />.
/// </typeparam>
/// <typeparam name="TResponse">
///   The type of the response produced by the query request.
/// </typeparam>
public class
  AuthorizationQueryHandlerExtension<THandler, TRequest, TResponse> : IQueryHandlerExtension<THandler, TRequest,
  TResponse> where THandler : IQueryHandler<TRequest, TResponse> where TRequest : IQueryRequest<TResponse>
{
  private readonly IAuthorizationService _authorizationService;
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly ILogger<IQueryHandlerExtension<THandler, TRequest, TResponse>> _logger;

  /// <summary>
  ///   Represents an extension for query handlers that integrates
  ///   authorization checks into the query handling pipeline.
  /// </summary>
  /// <typeparam name="THandler">
  ///   The type of the query handler that processes the query.
  /// </typeparam>
  /// <typeparam name="TRequest">
  ///   The type of the query request being handled.
  /// </typeparam>
  /// <typeparam name="TResponse">
  ///   The type of the response returned from handling the query.
  /// </typeparam>
  public AuthorizationQueryHandlerExtension(IHttpContextAccessor httpContextAccessor,
    IAuthorizationService authorizationService, ILogger<IQueryHandlerExtension<THandler, TRequest, TResponse>> logger)
  {
    _httpContextAccessor = httpContextAccessor;
    _authorizationService = authorizationService;
    _logger = logger;
  }

  /// <inheritdoc />
  public int? Order { get; } = 0;

  /// <summary>
  ///   Handles the query processing pipeline by integrating authorization checks
  ///   before delegating to the next handler in the pipeline.
  /// </summary>
  /// <param name="request">
  ///   The query request being processed.
  /// </param>
  /// <param name="handler">
  ///   The query handler that processes the request.
  /// </param>
  /// <param name="next">
  ///   The next handler in the pipeline to delegate processing after authorization checks.
  /// </param>
  /// <param name="cancellationToken">
  ///   A token for observing cancellation requests.
  /// </param>
  /// <returns>
  ///   The response of the processed query.
  /// </returns>
  /// <exception cref="UnauthorizedRequestException">
  ///   Thrown when the request is not authorized due to missing or invalid user context.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  ///   Thrown when unexpected errors occur during authorization checks.
  /// </exception>
  public async Task<TResponse> Handle(TRequest request, THandler handler,
    Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
  {
    await AuthorizationHelper.EnforceAuthorizationAsync(AuthorizationHelper.GetAttributes(typeof(THandler)),
      _httpContextAccessor, _authorizationService, _logger, request, typeof(TRequest), cancellationToken);

    cancellationToken.ThrowIfCancellationRequested();

    return await next.Invoke(request, cancellationToken);
  }

  /// <inheritdoc />
  public bool ShouldRun(TRequest request)
  {
    return AuthorizationHelper.GetAttributes(typeof(THandler)).Count > 0;
  }
}