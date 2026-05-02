using FluentValidation;
using FluentValidation.Results;
using RGamaFelix.CqrsDispatcher.Command;
using RGamaFelix.CqrsDispatcher.Command.Pipeline.Request;

namespace RGamaFelix.CqrsDispatcher.Validator;

/// <summary>
///   Represents a validation extension for command requests in a CQRS (Command Query Responsibility Segregation)
///   pipeline.
/// </summary>
/// <typeparam name="TRequest">The type of the command request. Must implement <see cref="ICommandRequest" />.</typeparam>
public sealed class CommandRequestValidator<TRequest> : ICommandRequestExtension<TRequest>
  where TRequest : ICommandRequest
{
  private readonly IEnumerable<IValidator<TRequest>> _validators;

  /// <summary>
  ///   Represents a behavior pipeline extension for handling command requests by performing validation using
  ///   FluentValidation.
  /// </summary>
  /// <typeparam name="TRequest">The type of the command request. Must implement <see cref="ICommandRequest" />.</typeparam>
  public CommandRequestValidator(IEnumerable<IValidator<TRequest>> validators)
  {
    _validators = validators;
  }

  /// <inheritdoc />
  public int? Order => 0;

  /// <summary>
  ///   Executes the validation pipeline for the provided command request.
  ///   Validates the request using all registered validators and invokes the subsequent pipeline delegate.
  ///   Throws a <see cref="ValidationException" /> if any validation errors occur.
  /// </summary>
  /// <param name="request">The command request to be validated.</param>
  /// <param name="next">The delegate representing the next step in the pipeline.</param>
  /// <param name="cancellationToken">A token to observe for cancellation of the operation.</param>
  /// <returns>A task representing the asynchronous operation.</returns>
  /// <exception cref="ValidationException">Thrown when validation fails for the command request.</exception>
  public async Task Handle(TRequest request, Func<TRequest, CancellationToken, Task> next,
    CancellationToken cancellationToken)
  {
    var failures = new List<ValidationFailure>();

    foreach (var validator in _validators)
    {
      var validationResult = await validator!.ValidateAsync(request, cancellationToken);

      if (!validationResult.IsValid)
      {
        failures.AddRange(validationResult.Errors);
      }
    }

    if (failures.Any())
    {
      throw new ValidationException(failures);
    }

    await next(request, cancellationToken);
  }

  /// <inheritdoc />
  public bool ShouldRun(TRequest request)
  {
    return _validators.Any();
  }
}