using FluentValidation;
using RGamaFelix.CqrsDispatcher.TestConsole.TestRequest;

namespace RGamaFelix.CqrsDispatcher.TestConsole.Validator;

public class BaseCommandRequestValidator : AbstractValidator<BaseCommandRequest>
{
  public BaseCommandRequestValidator()
  {
    RuleFor(p => p.IntValue)
      .LessThan(4)
      .WithMessage("Valor muito alto");
  }
}
