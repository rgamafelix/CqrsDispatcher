using FluentValidation;
using RGamaFelix.CqrsDispatcher.Query;
using RGamaFelix.CqrsDispatcher.Query.Extension.Request;

namespace RGamaFelix.CqrsDispatcher.Validator;

/// <summary>Represents a validator for query requests in the CQRS pattern, with support for FluentValidation.</summary>
/// <typeparam name="TRequest">The type of the query request to be validated, implementing IQueryRequest.</typeparam>
/// <typeparam name="TResponse">The type of the response produced by the query request.</typeparam>
/// <remarks>
///   This class is an implementation of the IQueryRequestExtension interface. It validates the incoming query
///   requests using the provided validators that implement FluentValidation's IValidator interface. If the validation
///   fails, a ValidationException is thrown.
/// </remarks>
public sealed class QueryRequestValidator<TRequest, TResponse> : IQueryRequestExtension<TRequest, TResponse>
  where TRequest : IQueryRequest<TResponse>
{
  private readonly IValidator<TRequest>? _validator;

  /// <summary>Provides a mechanism to validate query requests in a CQRS pattern using FluentValidation.</summary>
  /// <typeparam name="TRequest">The type of the query request being handled, which must implement IQueryRequest.</typeparam>
  /// <typeparam name="TResponse">The type of the response produced by the query request.</typeparam>
  /// <remarks>
  ///   This class integrates FluentValidation into the CQRS pipeline, enabling validation logic for incoming query
  ///   requests. If validation fails, a ValidationException is thrown. The validation is only performed if a matching
  ///   validator is provided. The class operates as an extension for the query-handling pipeline, ensuring compliance with
  ///   validation rules before the request is processed further.
  /// </remarks>
  public QueryRequestValidator(IEnumerable<IValidator<TRequest>> validators)
  {
    _validator = validators.FirstOrDefault();
  }

  /// inheritdoc
  public int? Order => 0;

  /// inheritdoc
  public async Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next,
    CancellationToken cancellationToken)
  {
    var validationResult = await _validator!.ValidateAsync(request, cancellationToken);

    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    return await next(request, cancellationToken);
  }

  /// inheritdoc
  public bool ShouldRun(TRequest request)
  {
    return _validator is not null;
  }
}
