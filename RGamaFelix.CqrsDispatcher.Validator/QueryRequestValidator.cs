using FluentValidation;
using RGamaFelix.CqrsDispatcher.Query;
using RGamaFelix.CqrsDispatcher.Query.Pipeline.Request;

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

  /// <inheritdoc />
  public int? Order => 0;

  /// <summary>Processes a query request by applying validation and forwarding it to the next handler in the pipeline.</summary>
  /// <param name="request">
  ///   The query request to process, which implements the <see cref="IQueryRequest{TResponse}" />
  ///   interface.
  /// </param>
  /// <param name="next">A delegate representing the next handler in the query pipeline to invoke after validation succeeds.</param>
  /// <param name="cancellationToken">
  ///   A token to observe while waiting for the task to complete, which may be used to cancel
  ///   the operation.
  /// </param>
  /// <returns>Returns the response produced by the query request after passing through the validation and pipeline.</returns>
  /// <exception cref="ValidationException">Thrown if the query request fails validation due to rule violations.</exception>
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

  /// <inheritdoc />
  public bool ShouldRun(TRequest request)
  {
    return _validator is not null;
  }
}