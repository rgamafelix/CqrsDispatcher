# Simple CQRS Dispatcher

A lightweight, easy-to-use CQRS (Command Query Responsibility Segregation) implementation inspired
by [MediatR](https://mediatr.io/). This library provides a clean and simple way to implement the CQRS pattern in your
.NET applications.

## ⚠️ Experimental Project Disclaimer

This project was developed as an experiment in AI-assisted software development. A significant
portion of the code was
generated and refined through interactions with AI language models. While functional, it should be considered
experimental and may not follow all best practices or be suitable for production use without careful review.

### AI Assistance Details

- The project structure and implementation were heavily guided by AI
- Code was iteratively refined through AI-human collaboration
- The development process itself served as an exploration of AI-assisted coding techniques

## Features

- 🚀 Simple and lightweight implementation
- ⚡ Async support out of the box
- 🔄 Separate handling for Commands and Queries
- 🔌 Easy integration with dependency injection
- 📦 Pipeline behavior support for cross-cutting concerns

## Getting Started

### Installation

Add the package to your project:

### Basic Setup

First, register the CQRS dispatcher in your dependency injection container:

``` csharp
services.AddScoped<Dispatcher>();
```

### Commands

Commands represent actions that change state in your application. They don't return values.

#### 1. Create a Command Request

``` csharp
public class CreateUserCommand : ICommandRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
}
```

#### 2. Create a Command Handler

``` csharp
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    private readonly IUserRepository _userRepository;
    
    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User(request.Name, request.Email);
        await _userRepository.SaveAsync(user, cancellationToken);
    }
}
```

#### 3. Register the Handler

``` csharp
services.AddScoped<ICommandHandler<CreateUserCommand>, CreateUserCommandHandler>();
```

#### 4. Execute the Command

``` csharp
public class UserController : ControllerBase
{
    private readonly Dispatcher _dispatcher;
    
    public UserController(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }
    
    [HttpPost]
    public IActionResult CreateUser([FromBody] CreateUserCommand command)
    {
        _dispatcher.Publish(command, ex => 
        {
            // Handle exceptions if needed
            _logger.LogError(ex, "Error creating user");
        });
        
        return Ok();
    }
}
```

### Queries

Queries represent read operations that return data without changing state.

#### 1. Create a Query Request

``` csharp
public class GetUserByIdQuery : IQueryRequest<UserDto>
{
    public int UserId { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
```

#### 2. Create a Query Handler

``` csharp
public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserRepository _userRepository;
    
    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<UserDto> HandleAsync(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        };
    }
}
```

#### 3. Register the Handler and Selector

``` csharp
services.AddScoped<IQueryHandler<GetUserByIdQuery, UserDto>, GetUserByIdQueryHandler>();
services.AddScoped<IQueryHandlerSelector<GetUserByIdQuery, UserDto>, DefaultFirstSelector<GetUserByIdQuery, UserDto>>();
```

#### 4. Execute the Query

``` csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(int id)
{
    var query = new GetUserByIdQuery { UserId = id };
    var user = await _dispatcher.Send<GetUserByIdQuery, UserDto>(query);
    return Ok(user);
}
```

### Pipeline Behaviors

Pipeline behaviors allow you to add cross-cutting concerns like logging, validation, or caching.

#### Request Behaviors

Request behaviors run before and after the handler execution:

``` csharp
public class LoggingCommandBehavior<TRequest> : ICommandRequestBehavior<TRequest>
    where TRequest : ICommandRequest
{
    private readonly ILogger<LoggingCommandBehavior<TRequest>> _logger;
    
    public LoggingCommandBehavior(ILogger<LoggingCommandBehavior<TRequest>> logger)
    {
        _logger = logger;
    }
    
    public int? Order => 0;
    
    public async Task Handle(TRequest request, Func<TRequest, CancellationToken, Task> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing command: {CommandType}", typeof(TRequest).Name);
        
        await next(request, cancellationToken);
        
        _logger.LogInformation("Command processed: {CommandType}", typeof(TRequest).Name);
    }
    
    public bool ShouldRun(TRequest request) => true;
}
```

#### Handler Behaviors

Handler behaviors wrap around specific handlers:

``` csharp
public class ValidationCommandBehavior<THandler, TRequest> : ICommandHandlerBehavior<THandler, TRequest>
    where THandler : ICommandHandler<TRequest>
    where TRequest : ICommandRequest
{
    private readonly IValidator<TRequest> _validator;
    
    public ValidationCommandBehavior(IValidator<TRequest> validator)
    {
        _validator = validator;
    }
    
    public int? Order => -1; // Run before other behaviors
    
    public async Task Handle(TRequest request, THandler handler, Func<TRequest, CancellationToken, Task> next, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
        
        await next(request, cancellationToken);
    }
    
    public bool ShouldRun(TRequest request) => true;
}
```

#### Register Behaviors

``` csharp
services.AddScoped(typeof(ICommandRequestBehavior<>), typeof(LoggingCommandBehavior<>));
services.AddScoped(typeof(ICommandHandlerBehavior<,>), typeof(ValidationCommandBehavior<,>));
```

### Multiple Handlers

For commands, you can register multiple handlers that will all execute:

``` csharp
services.AddScoped<ICommandHandler<CreateUserCommand>, CreateUserCommandHandler>();
services.AddScoped<ICommandHandler<CreateUserCommand>, SendWelcomeEmailHandler>();
services.AddScoped<ICommandHandler<CreateUserCommand>, LogUserCreationHandler>();
```

For queries, use the handler selector to choose which handler to execute:

``` csharp
public class UserQueryHandlerSelector : IQueryHandlerSelector<GetUserByIdQuery, UserDto>
{
    public IQueryHandler<GetUserByIdQuery, UserDto>? SelectHandler(
        GetUserByIdQuery request, 
        IEnumerable<IQueryHandler<GetUserByIdQuery, UserDto>> handlers)
    {
        // Custom logic to select the appropriate handler
        return handlers.FirstOrDefault(h => h.GetType() == typeof(CachedUserQueryHandler)) 
               ?? handlers.First();
    }
}
```

### Error Handling

Commands support error handling through the `onException` callback:

``` csharp
_dispatcher.Publish(command, exception =>
{
    // Log the error
    _logger.LogError(exception, "Command execution failed");
    
    // Optionally re-throw or handle the exception
    throw exception;
});
```

For queries, handle exceptions using standard try-catch blocks:

``` csharp
try
{
    var result = await _dispatcher.Send<GetUserByIdQuery, UserDto>(query);
    return Ok(result);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Query execution failed");
    return StatusCode(500, "Internal server error");
}
```
