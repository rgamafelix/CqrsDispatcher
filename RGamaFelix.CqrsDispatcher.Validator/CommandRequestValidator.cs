using FluentValidation;
using RGamaFelix.CqrsDispatcher.Command;
using RGamaFelix.CqrsDispatcher.Command.Extension.Request;

namespace RGamaFelix.CqrsDispatcher.Validator;

/// <summary>
///   Represents a validation extension for command requests in a CQRS (Command Query Responsibility Segregation)
///   pipeline.
/// </summary>
/// <typeparam name="TRequest">The type of the command request. Must implement <see cref="ICommandRequest" />.</typeparam>
public sealed class CommandRequestValidator<TRequest> : ICommandRequestExtension<TRequest>
  where TRequest : ICommandRequest
{
  private readonly IValidator<TRequest>? _validator;

  /// <summary>
  ///   Represents a behavior pipeline extension for handling command requests by performing validation using
  ///   FluentValidation.
  /// </summary>
  /// <typeparam name="TRequest">The type of the command request. Must implement <see cref="ICommandRequest" />.</typeparam>
  public CommandRequestValidator(IEnumerable<IValidator<TRequest>> validators)
  {
    _validator = validators.FirstOrDefault();
  }

  /// <inheritdoc />
  public int? Order => 0;

  /// <inheritdoc />
  public async Task Handle(TRequest request, Func<TRequest, CancellationToken, Task> next,
    CancellationToken cancellationToken)
  {
    var validationResult = await _validator!.ValidateAsync(request, cancellationToken);

    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    await next(request, cancellationToken);
  }

  /// <inheritdoc />
  public bool ShouldRun(TRequest request)
  {
    return _validator is not null;
  }
}
