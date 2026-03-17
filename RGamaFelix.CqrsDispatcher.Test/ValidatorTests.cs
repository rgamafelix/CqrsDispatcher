using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RGamaFelix.CqrsDispatcher.Command.Handler;
using RGamaFelix.CqrsDispatcher.Test.TestRequest;
using RGamaFelix.CqrsDispatcher.Validator;
using RGamaFelix.CqrsDispatcher.Validator.Configuration;

namespace RGamaFelix.CqrsDispatcher.Test;

public class ValidatorTests
{
  [Fact]
  public async Task CommandValidatorShouldCallNextWhenValidationPasses()
  {
    // Arrange
    var validator = Substitute.For<IValidator<TestCommandRequest>>();

    validator.ValidateAsync(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>())
      .Returns(new ValidationResult());

    var nextCalled = false;
    var sut = new CommandRequestValidator<TestCommandRequest>([validator]);

    Task Next(TestCommandRequest _, CancellationToken __)
    {
      nextCalled = true;

      return Task.CompletedTask;
    }

    // Act
    await sut.Handle(new TestCommandRequest(), Next, CancellationToken.None);

    // Assert
    Assert.True(nextCalled);
  }

  [Fact]
  public async Task CommandValidatorShouldNotRunWhenNoValidatorsRegistered()
  {
    // Arrange
    var sut = new CommandRequestValidator<TestCommandRequest>([]);

    // Act & Assert
    Assert.False(sut.ShouldRun(new TestCommandRequest()));
  }

  [Fact]
  public async Task CommandValidatorShouldRunWhenValidatorsAreRegistered()
  {
    // Arrange
    var validator = Substitute.For<IValidator<TestCommandRequest>>();
    var sut = new CommandRequestValidator<TestCommandRequest>([validator]);

    // Act & Assert
    Assert.True(sut.ShouldRun(new TestCommandRequest()));
  }

  [Fact]
  public async Task CommandValidatorShouldThrowWhenValidationFails()
  {
    // Arrange
    var validator = Substitute.For<IValidator<TestCommandRequest>>();
    var failures = new List<ValidationFailure> { new("Prop", "Error") };

    validator.ValidateAsync(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>())
      .Returns(new ValidationResult(failures));

    var sut = new CommandRequestValidator<TestCommandRequest>([validator]);

    // Act & Assert
    await Assert.ThrowsAsync<ValidationException>(() =>
      sut.Handle(new TestCommandRequest(), (_, _) => Task.CompletedTask, CancellationToken.None));
  }

  [Fact]
  public async Task QueryValidatorShouldCallNextWhenValidationPasses()
  {
    // Arrange
    var validator = Substitute.For<IValidator<TestQueryRequest>>();
    validator.ValidateAsync(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>()).Returns(new ValidationResult());
    var nextCalled = false;
    var sut = new QueryRequestValidator<TestQueryRequest, TestQueryResponse>([validator]);

    Task<TestQueryResponse> Next(TestQueryRequest _, CancellationToken __)
    {
      nextCalled = true;

      return Task.FromResult(new TestQueryResponse());
    }

    // Act
    await sut.Handle(new TestQueryRequest(), Next, CancellationToken.None);

    // Assert
    Assert.True(nextCalled);
  }

  [Fact]
  public async Task QueryValidatorShouldNotRunWhenNoValidatorsRegistered()
  {
    // Arrange
    var sut = new QueryRequestValidator<TestQueryRequest, TestQueryResponse>([]);

    // Act & Assert
    Assert.False(sut.ShouldRun(new TestQueryRequest()));
  }

  [Fact]
  public async Task QueryValidatorShouldRunWhenValidatorIsRegistered()
  {
    // Arrange
    var validator = Substitute.For<IValidator<TestQueryRequest>>();
    var sut = new QueryRequestValidator<TestQueryRequest, TestQueryResponse>([validator]);

    // Act & Assert
    Assert.True(sut.ShouldRun(new TestQueryRequest()));
  }

  [Fact]
  public async Task QueryValidatorShouldThrowWhenValidationFails()
  {
    // Arrange
    var validator = Substitute.For<IValidator<TestQueryRequest>>();
    var failures = new List<ValidationFailure> { new("Prop", "Error") };

    validator.ValidateAsync(Arg.Any<TestQueryRequest>(), Arg.Any<CancellationToken>())
      .Returns(new ValidationResult(failures));

    var sut = new QueryRequestValidator<TestQueryRequest, TestQueryResponse>([validator]);

    // Act & Assert
    await Assert.ThrowsAsync<ValidationException>(() => sut.Handle(new TestQueryRequest(),
      (_, _) => Task.FromResult(new TestQueryResponse()), CancellationToken.None));
  }

  [Fact]
  public async Task ValidatorExtensionShouldBeInvokedViaDispatcher()
  {
    // Arrange
    var services = TestHelper.CreateCleanServices();
    services.RegisterCqrsDispatcherValidator();
    var validator = Substitute.For<IValidator<TestCommandRequest>>();
    var failures = new List<ValidationFailure> { new("Prop", "Error") };

    validator.ValidateAsync(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>())
      .Returns(new ValidationResult(failures));

    var handler = Substitute.For<ICommandHandler<TestCommandRequest>>();
    handler.Handle(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
    services.AddScoped<IValidator<TestCommandRequest>>(_ => validator);
    services.AddScoped<ICommandHandler<TestCommandRequest>>(_ => handler);
    await using var provider = services.BuildServiceProvider();
    var dispatcher = new Dispatcher(provider, null);

    // Act & Assert
    await Assert.ThrowsAsync<ValidationException>(() => dispatcher.Publish(new TestCommandRequest()));
    await handler.DidNotReceive().Handle(Arg.Any<TestCommandRequest>(), Arg.Any<CancellationToken>());
  }
}