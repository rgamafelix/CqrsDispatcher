namespace RGamaFelix.CqrsDispatcher.Test.TestRequest;

public interface ISelectQueryHandler
{
  int ShouldSelect { get; }
}