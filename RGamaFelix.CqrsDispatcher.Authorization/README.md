# RGamaFelix.CqrsDispatcher.Authorization

An ASP.NET Core Authorization integration for the `RGamaFelix.CqrsDispatcher` framework. This package allows you to secure your command and query handlers using standard ASP.NET Core policies.

## Prerequisites

- `RGamaFelix.CqrsDispatcher` (Core project)
- `Microsoft.AspNetCore.Authorization`
- `Microsoft.AspNetCore.Http.Abstractions`

## Installation

Add the authorization project to your solution and ensure it references the core `RGamaFelix.CqrsDispatcher` project.

```bash
# Using dotnet CLI
dotnet add package RGamaFelix.CqrsDispatcher.Authorization
```

## Configuration

### 1. Register Extensions
In your `Program.cs` or `Startup.cs`, register the authorization extensions. This package provides helper methods to simplify policy creation.

```csharp
using RGamaFelix.CqrsDispatcher.Authorization.Configuration;

// ... in your ServiceCollection setup
services.AddCqrsDispatcherFramework(); // Base dispatcher

// Register the authorization pipeline extensions
services.AddAuthorizationExtensions(options => {
    // Standard ASP.NET Core Authorization options
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// Optional: Use built-in helpers for common policy types
services.AddRolePolicy("Manager", "ManagerRole")
        .AddClaimPolicy("VIP", "MemberType", "VIP")
        .AddAnyScopePolicy("ReadAccess", "api.read", "api.all");
```

### 2. Configure HttpContextAccessor
Since this package relies on `IHttpContextAccessor` to retrieve the current user, ensure it is registered:

```csharp
services.AddHttpContextAccessor();
```

## Usage Example

### 1. Apply Authorization to a Handler

Use the `[HandlerAuthorization]` attribute on your **Handler** class (not the request) to specify which policy is required to execute it.

```csharp
using RGamaFelix.CqrsDispatcher.Authorization;
using RGamaFelix.CqrsDispatcher.Command.Handler;

[HandlerAuthorization("AdminOnly")]
public class CreateUserHandler : ICommandHandler<CreateUserCommand>
{
    public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // This code only runs if the "AdminOnly" policy succeeds
        // ... implementation
    }
}
```

### 2. Handle Unauthorized Requests

If a user does not meet the policy requirements, the dispatcher will throw an `UnauthorizedRequestException`. You should handle this in your global exception filter or middleware.

```csharp
try 
{
    await _dispatcher.PublishAsync(new CreateUserCommand(...));
}
catch (UnauthorizedRequestException ex) 
{
    // Return 403 Forbidden or 401 Unauthorized
    return Forbid();
}
```

## Features

- **Attribute-Based Security:** Secure handlers individually using `[HandlerAuthorization("PolicyName")]`.
- **Standard Integration:** Uses the standard `IAuthorizationService`, making it compatible with all existing ASP.NET Core authorization logic.
- **Fluent Policy Helpers:** Includes extension methods for common scenarios (Roles, Claims, OAuth2 Scopes).
- **Handler-Level Granularity:** Authorization is checked at the handler level, allowing different handlers for the same request type to have different security requirements.
- **Automatic Enforcement:** The authorization check is part of the handler pipeline and is enforced automatically before the handler's `Handle` method is called.
