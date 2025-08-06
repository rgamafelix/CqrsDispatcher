namespace RGamaFelix.CqrsDispatcher.TestConsole.TestRequest;

public interface ISelectQueryHandler
{
  int ShouldSelect { get; }
}