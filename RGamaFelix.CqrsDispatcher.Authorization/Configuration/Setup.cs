using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using RGamaFelix.CqrsDispatcher.Command.Pipeline.Handler;
using RGamaFelix.CqrsDispatcher.Query.Pipeline.Handler;

namespace RGamaFelix.CqrsDispatcher.Authorization.Configuration;

/// <summary>
///   The Setup class provides extension methods for integrating authorization
///   capabilities into the command and query handling pipeline within
///   the RGamaFelix.CqrsDispatcher framework.
/// </summary>
public static class Setup
{
  private const string _scopeClaimType = "scope";
  private const string _scpClaimType = "scp";

  private static bool HasAnyRequiredValue(HashSet<string> requiredValues, IEnumerable<Claim> claimValues)
  {
    foreach (var claim in claimValues)
    {
      var parts = claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);

      foreach (var part in parts)
      {
        if (requiredValues.Contains(part))
        {
          return true;
        }
      }
    }

    return false;
  }

  private static void ThrowIfNullOrEmpty<T>(T[]? values, string paramName, string message)
  {
    if (values is null || values.Length == 0)
    {
      throw new ArgumentException(message, paramName);
    }
  }

  /// <summary>
  ///   Opinionated, reusable policy names for common scenarios.
  /// </summary>
  public static class CommonPolicyNames
  {
    public const string Admin = "Admin";
    public const string Authenticated = "Authenticated";
  }

  /// <param name="services">
  ///   The <see cref="IServiceCollection" /> to which the authorization handling
  ///   extensions will be added.
  /// </param>
  extension(IServiceCollection services)
  {
    /// <summary>
    ///   Registers a policy requiring at least one OAuth2 scope.
    ///   Works with both common claim shapes: "scope" (space-separated) and "scp" (single scope).
    /// </summary>
    public IServiceCollection AddAnyScopePolicy(string policyName, params string[] requiredScopes)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(policyName, nameof(policyName));
      ThrowIfNullOrEmpty(requiredScopes, nameof(requiredScopes), "At least one scope must be provided.");
      var required = new HashSet<string>(requiredScopes, StringComparer.OrdinalIgnoreCase);

      return services.AddAuthorizationCore(options => options.AddPolicy(policyName, policy =>
      {
        policy.RequireAuthenticatedUser();

        policy.RequireAssertion(ctx =>
          HasAnyRequiredValue(required, ctx.User.FindAll(_scopeClaimType)) ||
          HasAnyRequiredValue(required, ctx.User.FindAll(_scpClaimType)));
      }));
    }

    /// <summary>
    ///   Registers a policy requiring an authenticated user.
    /// </summary>
    public IServiceCollection AddAuthenticatedPolicy(string policyName)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(policyName);

      return services.AddAuthorizationCore(options =>
        options.AddPolicy(policyName, policy => policy.RequireAuthenticatedUser()));
    }

    /// <summary>
    ///   Adds extensions for handling authorization in command and query pipelines,
    ///   and configures the application's authorization policies.
    /// </summary>
    /// <param name="configure">
    ///   An optional delegate to configure the authorization options. If not provided, a default configuration is applied.
    /// </param>
    /// <returns>
    ///   The updated <see cref="IServiceCollection" /> with the registered authorization extensions.
    /// </returns>
    public IServiceCollection AddAuthorizationExtensions(Action<AuthorizationOptions>? configure = null)
    {
      services.AddScoped(typeof(ICommandHandlerExtension<,>), typeof(AuthorizationCommandHandlerExtension<,>));
      services.AddScoped(typeof(IQueryHandlerExtension<,,>), typeof(AuthorizationQueryHandlerExtension<,,>));

      if (configure is null)
      {
        services.AddAuthorizationCore();
      }
      else
      {
        services.AddAuthorizationCore(configure);
      }

      return services;
    }

    /// <summary>
    ///   Registers a policy requiring a claim. If <paramref name="allowedValues" /> are provided,
    ///   the claim must match at least one of them.
    /// </summary>
    public IServiceCollection AddClaimPolicy(string policyName, string claimType, params string[] allowedValues)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(policyName);
      ArgumentException.ThrowIfNullOrWhiteSpace(claimType);

      return services.AddAuthorizationCore(options => options.AddPolicy(policyName, policy =>
      {
        policy.RequireAuthenticatedUser();

        if (allowedValues is { Length: > 0 })
        {
          policy.RequireClaim(claimType, allowedValues);
        }
        else
        {
          policy.RequireClaim(claimType);
        }
      }));
    }

    /// <summary>
    ///   Registers a policy requiring an authenticated user with at least one of the given roles.
    /// </summary>
    public IServiceCollection AddRolePolicy(string policyName, params string[] requiredRoles)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(policyName);
      ThrowIfNullOrEmpty(requiredRoles, nameof(requiredRoles), "At least one role must be provided.");

      return services.AddAuthorizationCore(options => options.AddPolicy(policyName, policy =>
      {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(requiredRoles);
      }));
    }
  }
}
