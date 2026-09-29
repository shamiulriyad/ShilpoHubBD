using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using ShilpoHubBD.Api.Middlewares;

namespace ShilpoHubBD.UnitTests.Features.Platform.Middlewares;

[Trait("Feature", "Platform")]
[Trait("Layer", "Middleware")]
public class ValidationFilterTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    // Public: NSubstitute proxies IValidator<Thing>, which needs Thing itself to be accessible too.
    public sealed record Thing(string Name);

    private static (ValidationFilter Filter, ActionExecutingContext Context, bool[] NextCalled) Create(
        IServiceProvider services, Dictionary<string, object?> arguments)
    {
        var httpContext = new DefaultHttpContext { RequestServices = services };
        var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new ActionDescriptor());
        var nextCalled = new[] { false };
        var context = new ActionExecutingContext(
            actionContext, new List<IFilterMetadata>(), arguments, controller: new object());
        return (new ValidationFilter(services), context, nextCalled);
    }

    private static Task<ActionExecutedContext> Next(ActionExecutingContext context, bool[] called)
    {
        called[0] = true;
        return Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), context.Controller));
    }

    [Fact]
    public async Task OnActionExecutionAsync_ArgumentHasNoRegisteredValidator_CallsNextWithoutValidating()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var (filter, context, called) = Create(services, new Dictionary<string, object?> { ["thing"] = new Thing("x") });

        await filter.OnActionExecutionAsync(context, () => Next(context, called));

        Assert.True(called[0]);
    }

    [Fact]
    public async Task OnActionExecutionAsync_ValidArgument_CallsNext()
    {
        var validator = Substitute.For<IValidator<Thing>>();
        validator.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>()).Returns(new ValidationResult());
        var services = new ServiceCollection().AddSingleton(validator).BuildServiceProvider();
        var (filter, context, called) = Create(services, new Dictionary<string, object?> { ["thing"] = new Thing("x") });

        await filter.OnActionExecutionAsync(context, () => Next(context, called));

        Assert.True(called[0]);
    }

    [Fact]
    public async Task OnActionExecutionAsync_InvalidArgument_ThrowsValidationExceptionAndNeverCallsNext()
    {
        var validator = Substitute.For<IValidator<Thing>>();
        var failure = new ValidationFailure("Name", "Name is required.");
        validator.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>()).Returns(new ValidationResult(new[] { failure }));
        var services = new ServiceCollection().AddSingleton(validator).BuildServiceProvider();
        var (filter, context, called) = Create(services, new Dictionary<string, object?> { ["thing"] = new Thing("") });

        var error = await Assert.ThrowsAsync<ValidationException>(() => filter.OnActionExecutionAsync(context, () => Next(context, called)));

        Assert.Same(failure, Assert.Single(error.Errors));
        Assert.False(called[0]);
    }

    [Fact]
    public async Task OnActionExecutionAsync_NullArgument_IsSkipped()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var (filter, context, called) = Create(services, new Dictionary<string, object?> { ["thing"] = null });

        await filter.OnActionExecutionAsync(context, () => Next(context, called));

        Assert.True(called[0]);
    }

    [Fact]
    public async Task OnActionExecutionAsync_SeveralArguments_EachWithItsOwnValidator_AllAreValidated()
    {
        var thingValidator = Substitute.For<IValidator<Thing>>();
        thingValidator.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>()).Returns(new ValidationResult());
        var idValidator = Substitute.For<IValidator<Guid>>();
        idValidator.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Id", "Id must not be empty.") }));
        var services = new ServiceCollection().AddSingleton(thingValidator).AddSingleton(idValidator).BuildServiceProvider();
        var (filter, context, called) = Create(services, new Dictionary<string, object?> { ["thing"] = new Thing("x"), ["id"] = Guid.Empty });

        await Assert.ThrowsAsync<ValidationException>(() => filter.OnActionExecutionAsync(context, () => Next(context, called)));

        Assert.False(called[0]);
    }
}
