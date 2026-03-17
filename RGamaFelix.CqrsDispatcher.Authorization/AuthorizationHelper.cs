using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace RGamaFelix.CqrsDispatcher.Authorization;

internal static class AuthorizationHelper
{
  private static readonly ConcurrentDictionary<Type, IReadOnlyList<HandlerAuthorizationAttribute>> _cache = new();

  internal static async Task EnforceAuthorizationAsync<TRequest>(
    IReadOnlyList<HandlerAuthorizationAttribute> attributes, IHttpContextAccessor httpContextAccessor,
    IAuthorizationService authorizationService, ILogger logger, TRequest request, Type requestType,
    CancellationToken cancellationToken)
  {
    var user = httpContextAccessor.HttpContext?.User;

    if (user is null)
    {
      logger.LogWarning("Unauthorized request for {RequestType}. No HttpContext/User available.", requestType.Name);

      throw new UnauthorizedRequestException("No user context available for authorization.");
    }

    foreach (var attribute in attributes)
    {
      cancellationToken.ThrowIfCancellationRequested();
      var policyName = attribute.Policy;

      if (string.IsNullOrWhiteSpace(policyName))
      {
        logger.LogError("Authorization attribute on {RequestType} is missing Policy. Configure a policy name.",
          requestType.Name);

        throw new InvalidOperationException($"Authorization policy name is required on {requestType.Name}.");
      }

      var result = await authorizationService.AuthorizeAsync(user, request, policyName);

      if (!result.Succeeded)
      {
        logger.LogWarning("Unauthorized request for {RequestType}. Policy '{Policy}' failed.", requestType.Name,
          policyName);

        throw new UnauthorizedRequestException($"Authorization policy '{policyName}' failed.");
      }
    }
  }

  internal static IReadOnlyList<HandlerAuthorizationAttribute> GetAttributes(Type handlerType)
  {
    return _cache.GetOrAdd(handlerType,
      static t => t.GetCustomAttributes(typeof(HandlerAuthorizationAttribute), true)
        .Cast<HandlerAuthorizationAttribute>()
        .ToList());
  }
}