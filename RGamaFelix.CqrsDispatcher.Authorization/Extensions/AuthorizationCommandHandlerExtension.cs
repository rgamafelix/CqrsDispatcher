using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RGamaFelix.CqrsDispatcher.Command;
using RGamaFelix.CqrsDispatcher.Command.Handler;
using RGamaFelix.CqrsDispatcher.Command.Pipeline.Handler;

namespace RGamaFelix.CqrsDispatcher.Authorization;

/// <summary>
///   Provides an extension for adding authorization checks to command handlers within the
///   CQRS pipeline. This extension intercepts the handling of a command request to ensure
///   that the current user is authorized to execute the associated command.
/// </summary>
/// <typeparam name="THandler">
///   The type of the command handler responsible for handling the specific command request.
///   Must implement the <see cref="ICommandHandler{TRequest}" /> interface.
/// </typeparam>
/// <typeparam name="TRequest">
///   The type of the command request. Must implement the <see cref="ICommandRequest" /> interface.
/// </typeparam>
public class AuthorizationCommandHandlerExtension<THandler, TRequest> : ICommandHandlerExtension<THandler, TRequest>
  where THandler : ICommandHandler<TRequest> where TRequest : ICommandRequest
{
  private readonly IAuthorizationService _authorizationService;
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly ILogger<ICommandHandlerExtension<THandler, TRequest>> _logger;

  /// <summary>
  ///   Provides an authorization extension for command handlers by utilizing the
  ///   ASP.NET Core authorization framework. This extension ensures that authorization
  ///   checks are performed before executing the specified command handler.
  /// </summary>
  /// <typeparam name="THandler">The type of the command handler.</typeparam>
  /// <typeparam name="TRequest">The type of the command request.</typeparam>
  public AuthorizationCommandHandlerExtension(IHttpContextAccessor httpContextAccessor,
    IAuthorizationService authorizationService, ILogger<ICommandHandlerExtension<THandler, TRequest>> logger)
  {
    _httpContextAccessor = httpContextAccessor;
    _authorizationService = authorizationService;
    _logger = logger;
  }

  /// <inheritdoc />
  public int? Order { get; } = 0;

  /// <summary>
  ///   Executes the handling pipeline for a command, applying authorization policies
  ///   before proceeding to the next delegate in the pipeline. This method ensures
  ///   that the user is authenticated and authorized based on the attributes defined
  ///   on the associated handler.
  /// </summary>
  /// <param name="request">The command request to be processed.</param>
  /// <param name="handler">The command handler responsible for processing the request.</param>
  /// <param name="next">The delegate representing the next step in the pipeline.</param>
  /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
  /// <returns>A task representing the asynchronous operation of the command pipeline.</returns>
  /// <exception cref="UnauthorizedRequestException">Thrown when the user is not authorized to perform the request.</exception>
  /// <exception cref="InvalidOperationException">Thrown when required context or parameters are invalid during execution.</exception>
  public async Task Handle(TRequest request, THandler handler, Func<TRequest, CancellationToken, Task> next,
    CancellationToken cancellationToken)
  {
    await AuthorizationHelper.EnforceAuthorizationAsync(AuthorizationHelper.GetAttributes(typeof(THandler)),
      _httpContextAccessor, _authorizationService, _logger, request, typeof(TRequest), cancellationToken);

    cancellationToken.ThrowIfCancellationRequested();
    await next.Invoke(request, cancellationToken);
  }

  /// <inheritdoc />
  public bool ShouldRun(TRequest request)
  {
    return AuthorizationHelper.GetAttributes(typeof(THandler)).Count > 0;
  }
}