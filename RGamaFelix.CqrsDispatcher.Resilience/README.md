# RGamaFelix.CqrsDispatcher.Resilience

A resilience and retry extension for the `RGamaFelix.CqrsDispatcher` framework. This package specifically targets **Event Handlers**, allowing them to automatically retry upon failure based on a declarative policy.

## Prerequisites

- `RGamaFelix.CqrsDispatcher` (Core project)

## Installation

Add the resilience project to your solution and ensure it references the core `RGamaFelix.CqrsDispatcher` project.

```bash
# Using dotnet CLI
dotnet add package RGamaFelix.CqrsDispatcher.Resilience
```

## Configuration

To enable retry capabilities, register the resilience extensions in your `Program.cs` or `Startup.cs`:

```csharp
using RGamaFelix.CqrsDispatcher.Resilience.Configuration;

// ... in your ServiceCollection setup
services.AddCqrsDispatcherFramework(); // Base dispatcher
services.AddRetryExtensions();          // Register the retry pipeline for event handlers
```

## Usage Example

### 1. Apply a Retry Policy to an Event Handler

Use the `[RetryPolicy]` attribute on your **Event Handler** class. You can specify the maximum number of attempts and an optional delay between them.

```csharp
using RGamaFelix.CqrsDispatcher.Resilience;
using RGamaFelix.CqrsDispatcher.Event.Handler;

// This handler will be retried up to 3 times with a 500ms delay between attempts
[RetryPolicy(maxAttempts: 3, DelayMilliseconds = 500)]
public class SendWelcomeEmailHandler : IEventHandler<UserCreatedEvent>
{
    public async Task Handle(UserCreatedEvent @event, CancellationToken cancellationToken)
    {
        // If this throws an exception, the pipeline will catch it and retry
        await _emailService.SendAsync(@event.Email, "Welcome!");
    }
}
```

### 2. Handling Exhausted Retries

If all retry attempts fail, the dispatcher will throw an `EventHandlerRetryExhaustedException`. This exception contains details about the handler that failed and the list of exceptions encountered during each attempt.

```csharp
try 
{
    await _dispatcher.PublishEventAsync(new UserCreatedEvent("user@example.com"));
}
catch (EventHandlerRetryExhaustedException ex) 
{
    Console.WriteLine($"Handler {ex.HandlerType.Name} failed after {ex.Attempts} attempts.");
    foreach (var innerEx in ex.InnerExceptions)
    {
        Console.WriteLine($"- Error: {innerEx.Message}");
    }
}
```

## Features

- **Declarative Retry Logic:** Simply decorate your event handlers with `[RetryPolicy]`.
- **Customizable Delays:** Support for fixed delays between retry attempts.
- **Automatic Integration:** Plugs into the existing `IEventHandlerExtension` pipeline.
- **Detailed Error Reporting:** `EventHandlerRetryExhaustedException` provides access to all exceptions that occurred during the retry cycle.
- **Non-Intrusive:** If no `[RetryPolicy]` is present on a handler, it executes normally without any overhead.
