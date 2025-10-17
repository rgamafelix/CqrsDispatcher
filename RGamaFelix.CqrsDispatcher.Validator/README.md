# RGamaFelix.CqrsDispatcher.Validator

A FluentValidation integration extension for the RGamaFelix.CqrsDispatcher library, providing automatic validation of
command requests in the CQRS pipeline.

## Overview

This package extends the CQRS Dispatcher library by adding FluentValidation support for command requests. It
automatically validates command requests before they are processed by handlers, ensuring data integrity and business
rule enforcement at the pipeline level.

## Installation

```bash
  dotnet add package RGamaFelix.CqrsDispatcher.Validator
```

## Features

- 🔍 **Automatic Validation**: Integrates seamlessly with FluentValidation to validate command requests
- 🔄 **Pipeline Integration**: Works as a request extension in the CQRS pipeline
- ⚡ **Early Validation**: Validates requests before they reach handlers
- 🎯 **Type-Safe**: Strongly typed validation for each command request type
- 🔧 **Configurable Order**: Runs at the beginning of the pipeline (Order = 0)

## Usage

### 1. Define Your Command Request

```csharp
public class CreateUserCommand : ICommandRequest 
{
    public string Username { get; set; } 
    public string Email { get; set; } 
    public int Age { get; set; }
} 
```

### 2. Create a FluentValidation Validator

```csharp
public class CreateUserCommandValidator : AbstractValidator
{
    public CreateUserCommandValidator() 
    {
        RuleFor(x =>x.Username).NotEmpty().MinimumLength(3).MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Age).GreaterThanOrEqualTo(18).WithMessage("User must be at least 18 years old");
    }
}
```

### 3. Register Services

```csharp
 services.AddScoped<IValidator, CreateUserCommandValidator>();
 services.AddScoped(typeof(ICommandRequestExtension<>), typeof(CommandRequestValidator<>));
``` 

### 4. Execute Commands

When you dispatch a command, validation will automatically occur:

```csharp
var command = new CreateUserCommand 
{
    Username = "john_doe",
    Email = "john@example.com", Age = 25
};
// Validation happens automatically in the pipeline
await dispatcher.DispatchAsync(command, cancellationToken);
```

If validation fails, a `FluentValidation.ValidationException` is thrown with detailed error information.

## How It Works

The `CommandRequestValidator<TRequest>` class:

1. **Injection**: Accepts `IEnumerable<IValidator<TRequest>>` in the constructor
2. **Execution**: Runs at the beginning of the pipeline (Order = 0)
3. **Validation**: Calls `ValidateAsync` on the registered validator
4. **Error Handling**: Throws `ValidationException` if validation fails
5. **Pass-Through**: Calls `next()` to continue the pipeline if validation succeeds

## Error Handling

When validation fails, a `ValidationException` is thrown containing all validation errors:

```csharp
try 
{
    await dispatcher.DispatchAsync(command, cancellationToken);
} 
catch (ValidationException ex) 
{
    foreach (varerror in ex.Errors)
    {
        Console.WriteLine($"{error.PropertyName}: {error.ErrorMessage}");
    } 
}
```

## Requirements

- .NET 9.0 or higher
- RGamaFelix.CqrsDispatcher
- FluentValidation

## Dependencies

This package depends on:

- `RGamaFelix.CqrsDispatcher` - Core CQRS dispatcher library
- `FluentValidation` - Validation library

## License

[Your License Here]

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For issues, questions, or contributions, please visit
the [GitHub repository](https://github.com/RGamaFelix/CqrsDispatcher).
