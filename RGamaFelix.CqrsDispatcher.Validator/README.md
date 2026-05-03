# RGamaFelix.CqrsDispatcher.Validator

A FluentValidation integration for the `RGamaFelix.CqrsDispatcher` framework. This package provides automatic validation for commands and queries using the pipeline behavior pattern.

## Prerequisites

- `RGamaFelix.CqrsDispatcher` (Core project)
- `FluentValidation`

## Installation

Add the validator project to your solution and ensure it references the core `RGamaFelix.CqrsDispatcher` project.

```bash
# Using dotnet CLI
dotnet add package RGamaFelix.CqrsDispatcher.Validator
```

## Configuration

To enable automatic validation, register the validator extensions in your `Program.cs` or `Startup.cs`:

```csharp
using RGamaFelix.CqrsDispatcher.Validator.Configuration;

// ... in your ServiceCollection setup
services.AddCqrsDispatcherFramework(); // Base dispatcher
services.RegisterCqrsDispatcherValidator(); // This package's validation pipeline
```

You also need to register your `FluentValidation` validators. The dispatcher will automatically pick up any `IValidator<T>` registered in the DI container for your request types.

```csharp
using FluentValidation;

// Register all validators from an assembly
services.AddValidatorsFromAssemblyContaining<MyCommandValidator>();
```

## Usage Example

### 1. Define your Request and Validator

```csharp
using FluentValidation;
using RGamaFelix.CqrsDispatcher.Command;

public record CreateUserCommand(string Username, string Email) : ICommandRequest;

public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
```

### 2. Dispatch and Handle Validation Errors

When you dispatch a command or query, the `CommandRequestValidator` or `QueryRequestValidator` will intercept the request, run all registered validators, and throw a `FluentValidation.ValidationException` if any validation fails.

```csharp
try 
{
    var command = new CreateUserCommand("", "invalid-email");
    await _dispatcher.PublishAsync(command);
}
catch (ValidationException ex) 
{
    // Handle validation failures (e.g., return 400 Bad Request)
    foreach (var error in ex.Errors)
    {
        Console.WriteLine($"{error.PropertyName}: {error.ErrorMessage}");
    }
}
```

## Features

- **Automatic Pipeline Integration:** Plugs into the CQRS dispatcher pipeline without manual calls to `Validate()`.
- **Supports Multiple Validators:** Runs all registered validators for a specific request type.
- **Async Validation:** Uses FluentValidation's `ValidateAsync` for non-blocking validation.
- **Transparent Execution:** If no validators are registered for a type, the pipeline continues normally.
