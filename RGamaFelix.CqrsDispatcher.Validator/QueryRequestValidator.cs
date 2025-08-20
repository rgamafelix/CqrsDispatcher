using FluentValidation;
using RGamaFelix.CqrsDispatcher.Query;
using RGamaFelix.CqrsDispatcher.Query.Pipeline.Request;

namespace RGamaFelix.CqrsDispatcher.Validator;

public class QueryRequestValidator<TRequest, TResponse> : IQueryRequestBehavior<TRequest, TResponse>
  where TRequest : IQueryRequest<TResponse>
{
  protected readonly IValidator<TRequest> Validator;

  public QueryRequestValidator(IEnumerable<IValidator<TRequest>> validators)
  {
    Validator = validators.FirstOrDefault() ?? new DefaultValidator<TRequest>();
  }

  public virtual int? Order => 0;

  public virtual async Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next,
    CancellationToken cancellationToken)
  {
    var validationResult = await Validator.ValidateAsync(request, cancellationToken);

    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    return await next(request, cancellationToken);
  }

  public virtual bool ShouldRun(TRequest request)
  {
    return true;
  }
}