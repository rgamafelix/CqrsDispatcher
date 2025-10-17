# RGamaFelix.RequestDispatcher

A powerful .NET library that implements the **Request-Response pattern** with support for **Command** and **Query**
operations, inspired by the mediator pattern but with enhanced functionality including behavior pipelines and handler
selection mechanisms.

## Features

- **🔄 Command/Query Separation (CQS)**: Clear separation between commands (write operations) and queries (read
  operations)
- **🔗 Behavior Pipelines**: Composable middleware-style behaviors for cross-cutting concerns
- **🎯 Handler Selection**: Smart handler selection for scenarios with multiple handlers
- **⚡ Asynchronous Processing**: Full async/await support with cancellation tokens
- **📊 Structured Logging**: Built-in logging with scoped contexts
- **🏗️ Dependency Injection**: Full integration with Microsoft.Extensions.DependencyInjection
- **🔧 Thread-Safe**: Optimized with concurrent dictionaries for reflection caching
- **✅ Validation Support**: Built-in support for FluentValidation

## Installation

```bash
  Install-Package RGamaFelix.RequestDispatcher
```

Or via .NET CLI:

```bash
  dotnet add package RGamaFelix.RequestDispatcher
``` 

## Quick Start

### 1. Register the Dispatcher

```csharp 
import RGamaFelix.CqrsDispatcher.Configuration;

services.AddCqrsDispatcherFramework();
```

### 2. Define Your Requests

Requests can defined as classes or records, but the recommended approach is to use records because of their immutable
nature.

**Command (No Response)**:

```csharp
public record CreateUserCommand(string Name, string Email) : ICommandRequest;
``` 

**Query (With Response)**:

```csharp
public record GetUserQuery(int UserId) : IQueryRequest<UserDto>;

public record UserDto(int Id, string Name, string Email);
```

### 3. Create Handlers

**Command Handler**:

```csharp
public class CreateUserHandler : ICommandHandler<CreateUserCommand>
{
    public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Your business logic here
        await CreateUserInDatabase(request.Name, request.Email); 
    }
}
```

**Query Handler**:

```csharp
public class GetUserHandler : IQueryHandler<GetUserQuery, UserDto>
{
    public async TaskHandleAsync(GetUserQuery request, CancellationToken cancellationToken)
    {
        // Your query logic here
        return await GetUserFromDatabase(request.UserId); 
    }
}
```

### 4. Use the Dispatcher

```csharp
public class UserController : ControllerBase
{
    private readonly Dispatcher _dispatcher;
    public UserController(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserCommand command)
    {
        await _dispatcher.Publish(command);
        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<UserDto> GetUser(int id)
    {
        var query = new GetUserQuery { UserId = id };
        return await _dispatcher.Send<GetUserQuery, UserDto>(query);
    }
}
```

## Advanced Features

### Pipelines Extensions

Extensions allow you to implement cross-cutting concerns like validation, logging, caching, etc.

**Request Behavior** (applies to the entire request):

```csharp
public class LoggingBehavior: ICommandRequestBehavior 
    where TRequest : IRequest 
{
    public int? Order => 1;
    public async Task Handle(TRequest request, Func<TRequest, CancellationToken, Task> next, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Processing {typeof(TRequest).Name}");
        await next(request, cancellationToken);
        Console.WriteLine($"Finished {typeof(TRequest).Name}");
    }
}
```

**Handler Behavior** (applies to specific handlers):

```csharp
public class CachingBehavior<THandler, TRequest, TResponse>
    : IQueryHandlerBehavior<THandler, TRequest, TResponse>
    where TRequest : IRequestwhere THandler : IQueryHandler 
{
    public async Task Handle(TRequest request, THandler handler, Func next, CancellationToken cancellationToken) 
    {
        // Check cache first
        var cached = GetFromCache(request);
        if (cached != null) return cached;
        var result = await next(request, cancellationToken);
        
        // Cache the result
        SetCache(request, result);
        return result;
    }
}
```

### Validation with FluentValidation

Optional validation support is available through the package: `RGamaFelix.CqrsDispatcher.Validator

```` bash
    dotnet add package RGamaFelix.CqrsDispatcher.Validator
````

```csharp
public class CreateUserValidator : AbstractValidator
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress(); 
    }
}

// Register the validator behavior
services.AddTransient<ICommandRequestBehavior, CommandRequestValidator >();
```

### Conditional Behaviors

Behaviors can implement conditional logic:

```csharp
public class AuthorizationBehavior: ICommandRequestBehavior where TRequest : IRequest
{
    public bool ShouldRun(TRequest request) 
    {
        // Only run for requests that implement IAuthorizedRequest
        return request is IAuthorizedRequest; 
    }
    
    public async Task Handle(TRequest request, Func<TRequest, CancellationToken, Task> next, CancellationToken 
    cancellationToken)
    {
        // Authorization logic here
        if (!IsAuthorized(request))
            throw new UnauthorizedAccessException();
        
        await next(request, cancellationToken);
    }
}
```

### Handler Selection

For scenarios where multiple handlers exist for the same request:

```csharp
public class DatabaseQueryHandlerSelector : IQueryHandlerSelector<GetUserQuery, UserDto>
{
    public IQueryHandler<GetUserQuery, UserDto> SelectHandler(GetUserQuery request, 
    IEnumerable<IQueryHandler<GetUserQuery, UserDto>> handlers)
    {
        // Custom logic to select the appropriate handler
        return request.UseCache ? handlers.OfType().First() : handlers.OfType().First(); 
    } 
}
```

## Error Handling

The dispatcher provides built-in error handling with optional custom exception callbacks:

```csharp
await _dispatcher.Publish(command, exception =>
{
    // Custom error handling
    _logger.LogError(exception, "Command failed");
    // Send notification, etc.
});
```

## Performance Considerations

- Reflection Caching: The dispatcher uses ConcurrentDictionary to cache reflection operations
- Scoped Services: Each request creates a new service scope for proper dependency lifecycle management
- Async Processing: Commands are processed asynchronously using Task.Run
- Parallel Execution: Multiple command handlers are executed in parallel

- ## Logging Integration

The dispatcher integrates with Microsoft.Extensions.Logging:

```csharp
services.AddLogging();
services.AddSingleton<Dispatcher>();
```

Structured logging is automatically provided with request type context.

## Dependencies

- .NET 9.0+
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Logging

## License

This project is licensed under the MIT License.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## API Reference

### Core Methods

- Task Publish<TRequest>(TRequest request, Action<Exception>? onException = null, CancellationToken cancellationToken =
  default) - Publishes a command for asynchronous processing
- Task<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default) - Sends a
  query and returns the response

#### Interfaces

- ICommandRequest - Marker interface for commands
- IQueryRequest<TResponse> - Interface for queries with response
- ICommandHandler<TRequest> - Interface for command handlers
- IQueryHandler<TRequest, TResponse> - Interface for query handlers
- ICommandRequestBehavior<TRequest> - Interface for command request behaviors
- IQueryRequestBehavior<TRequest, TResponse> - Interface for query request behaviors
- ICommandHandlerBehavior<THandler, TRequest> - Interface for command handler behaviors
- IQueryHandlerBehavior<THandler, TRequest, TResponse> - Interface for query handler behaviors
- IQueryHandlerSelector<TRequest, TResponse> - Interface for handler selection logic
