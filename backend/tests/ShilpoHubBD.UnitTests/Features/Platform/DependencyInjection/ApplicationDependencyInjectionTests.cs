using Microsoft.Extensions.DependencyInjection;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Platform.DependencyInjection;

[Trait("Feature", "Platform")]
[Trait("Layer", "DI registration")]
public class ApplicationDependencyInjectionTests
{
    [Fact]
    public void AddApplication_RegistersFluentValidationValidatorsFromItsOwnAssembly()
    {
        var (provider, _, _, _) = CompositeContainer.Build();
        using var scope = provider.CreateScope();

        var loginValidator = scope.ServiceProvider.GetService<FluentValidation.IValidator<ShilpoHubBD.Application.DTOs.Auth.LoginRequest>>();

        Assert.NotNull(loginValidator);
    }

    [Fact]
    public void AddApplication_RegistersMoreThanOneHundredServices()
    {
        var (_, _, _, applicationTypes) = CompositeContainer.Build();

        var services = applicationTypes.Where(t => t.Name.EndsWith("Service", StringComparison.Ordinal)).ToList();

        Assert.True(services.Count > 100, $"Expected over 100 service registrations, found {services.Count}.");
    }

    [Fact]
    public void AddApplication_RegistersAuthAndSecurityServicesUsedByThisTestSuite()
    {
        var (provider, _, _, applicationTypes) = CompositeContainer.Build();
        using var scope = provider.CreateScope();

        Assert.Contains(typeof(IAuthService), applicationTypes);
        Assert.Contains(typeof(IRoleService), applicationTypes);
        Assert.Contains(typeof(IApiKeyService), applicationTypes);
        Assert.NotNull(scope.ServiceProvider.GetService<IAuthService>());
        Assert.NotNull(scope.ServiceProvider.GetService<IApiKeyService>());
    }

    [Fact]
    public void AddApplication_EveryRegisteredServiceResolvesAgainstDataAndInfrastructure()
    {
        // Application services depend on repository interfaces (Data) and provider interfaces
        // (Infrastructure), so this exercises the full three-layer graph the way Program.cs builds it,
        // not AddApplication in isolation.
        var (provider, _, _, applicationTypes) = CompositeContainer.Build();
        using var scope = provider.CreateScope();

        var failures = applicationTypes
            .Select(type => (type, error: TryResolve(scope.ServiceProvider, type)))
            .Where(r => r.error is not null)
            .ToList();

        Assert.True(failures.Count == 0,
            "The following Application-layer services failed to resolve:\n" + string.Join('\n', failures.Select(f => $"  {f.type.FullName}: {f.error}")));
    }

    private static string? TryResolve(IServiceProvider provider, Type serviceType)
    {
        try
        {
            return provider.GetService(serviceType) is null ? "resolved to null" : null;
        }
        catch (Exception ex)
        {
            return ex.GetBaseException().Message;
        }
    }
}
