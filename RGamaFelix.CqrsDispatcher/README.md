# RGamaFelix.CqrsDispatcher

[![NuGet Version](https://img.shields.io/nuget/v/RGamaFelix.CqrsDispatcher.svg)](https://www.nuget.org/packages/RGamaFelix.CqrsDispatcher)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A lightweight, high-performance, and extensible asynchronous CQRS implementation for .NET.

## Presentation

`RGamaFelix.CqrsDispatcher` is a decoupled messaging library inspired by the mediator pattern. it provides a clean and structured way to implement **Command Query Responsibility Segregation (CQRS)** in your applications. By separating state-changing operations (Commands) from data-retrieval operations (Queries) and providing a robust Event notification system, it helps you build maintainable and scalable systems.

## Intent & Core Principles

- **Decoupling**: Business logic is separated from the transport layer (API, CLI, etc.) and other infrastructure concerns.
- **Extensibility**: A powerful pipeline system allows adding cross-cutting concerns (validation, logging, authorization, caching) without modifying the core logic.
- **Async-First**: Built from the ground up for asynchronous execution to maximize throughput and responsiveness.
- **Type Safety**: Leverages C# generics and interfaces to ensure compile-time safety for all requests and handlers.

## Common Use Cases

- **Clean Architecture / Hexagonal Architecture**: Implement your use cases as Commands and Queries.
- **Microservices**: Decouple internal service logic.
- **Modular Monoliths**: Use the dispatcher to communicate between different modules.
- **Event-Driven Architectures**: Leverage the `Notify` system for fire-and-forget notifications.

## Technologies Used

- **.NET 8.0 / 9.0 / 10.0**
- **Microsoft.Extensions.DependencyInjection**: Deeply integrated for automatic service resolution.
- **Microsoft.Extensions.Logging**: Built-in support for structured logging.
- **ConcurrentDictionary & Reflection Caching**: Optimized for high-performance dispatching.

## Installation

Install via NuGet:

```bash
dotnet add package RGamaFelix.CqrsDispatcher
```

## Quick Start

### 1. Register the Framework

In your `Program.cs` or startup configuration:

```csharp
using RGamaFelix.CqrsDispatcher.Configuration;

// Add the core dispatcher
builder.Services.AddCqrsDispatcherFramework();

// Auto-scan and register handlers and extensions from your assembly
builder.Services.RegisterCqrsDispatcherComponents(typeof(Program).Assembly);
```

### 2. Commands (State Changes)

Commands represent an intent to change the state of the system. They are processed by exactly one handler.

```csharp
// Define the Command
public record CreateUserCommand(string Username, string Email) : ICommandRequest;

// Implement the Handler
public class CreateUserHandler : ICommandHandler<CreateUserCommand>
{
    public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Business logic to create user
        await Task.CompletedTask;
    }
}
```

### 3. Queries (Data Retrieval)

Queries represent a request for data and always return a response. They are processed by exactly one handler.

```csharp
// Define the Query and Response
public record GetUserQuery(Guid UserId) : IQueryRequest<UserResponse>;
public record UserResponse(Guid Id, string Username);

// Implement the Handler
public class GetUserHandler : IQueryHandler<GetUserQuery, UserResponse>
{
    public async Task<UserResponse> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        // Logic to fetch user
        return new UserResponse(request.UserId, "JohnDoe");
    }
}
```

### 4. Events (Notifications)

Events are fire-and-forget notifications that can be processed by zero or more handlers concurrently.

```csharp
// Define the Event
public record UserCreatedEvent(Guid UserId) : IEvent;

// Implement Handlers (multiple handlers can listen to the same event)
public class SendWelcomeEmailHandler : IEventHandler<UserCreatedEvent>
{
    public async Task Handle(UserCreatedEvent @event, CancellationToken cancellationToken)
    {
        // Logic to send email
        await Task.CompletedTask;
    }
}
```

### 5. Dispatching

Inject the `Dispatcher` into your controllers or services:

```csharp
public class UsersController(Dispatcher dispatcher) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateUserCommand command)
    {
        await dispatcher.Publish(command);
        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<UserResponse> Get(Guid id)
    {
        return await dispatcher.Send<GetUserQuery, UserResponse>(new GetUserQuery(id));
    }

    [HttpPost("notify")]
    public async Task Notify(Guid id)
    {
        await dispatcher.Notify(new UserCreatedEvent(id));
    }
}
```

## Advanced Features

### Pipelines (Extensions)

You can wrap request execution or handler execution with "Extensions". These are similar to middleware.

- **Request Extensions**: Run for every request of a specific type.
- **Handler Extensions**: Run only when a specific handler is executed.

#### Request Extension Example (Validation)

```csharp
public class ValidationExtension<TRequest> : ICommandRequestExtension<TRequest> 
    where TRequest : ICommandRequest
{
    public int? Order => 1; // Lower runs first

    public async Task Handle(TRequest request, Func<TRequest, CancellationToken, Task> next, CancellationToken ct)
    {
        // Perform validation here
        await next(request, ct);
    }
}
```

### Handler Selection

If you have multiple handlers for the same Query, you can register an `IQueryHandlerSelector` to decide which one to use at runtime based on the request content.

### Conditional Execution

Extensions can implement a `ShouldRun(TRequest request)` method to conditionally execute their logic.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
