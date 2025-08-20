using FluentValidation;
using RGamaFelix.CqrsDispatcher.Command;
using RGamaFelix.CqrsDispatcher.Command.Pipeline.Request;

namespace RGamaFelix.CqrsDispatcher.Validator;

public class CommandRequestValidator<TRequest> : ICommandRequestBehavior<TRequest>
  where TRequest : ICommandRequest
{
  protected readonly IValidator<TRequest> Validator;

  public CommandRequestValidator(IEnumerable<IValidator<TRequest>> validators)
  {
    Validator = validators.FirstOrDefault() ?? new DefaultValidator<TRequest>();
  }

  public virtual int? Order => 0;

  public virtual async Task Handle(TRequest request, Func<TRequest, CancellationToken, Task> next,
    CancellationToken cancellationToken)
  {
    var validationResult = await Validator.ValidateAsync(request, cancellationToken);

    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    await next(request, cancellationToken);
  }

  public virtual bool ShouldRun(TRequest request)
  {
    return true;
  }
}