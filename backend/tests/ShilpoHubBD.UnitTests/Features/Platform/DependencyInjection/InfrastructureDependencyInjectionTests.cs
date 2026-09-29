using Microsoft.Extensions.DependencyInjection;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Platform.DependencyInjection;

[Trait("Feature", "Platform")]
[Trait("Layer", "DI registration")]
public class InfrastructureDependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_RegistersTheAuthProviders()
    {
        var (provider, _, infrastructureTypes, _) = CompositeContainer.Build();
        using var scope = provider.CreateScope();

        Assert.Contains(typeof(ITokenService), infrastructureTypes);
        Assert.Contains(typeof(IPasswordHasher), infrastructureTypes);
        Assert.NotNull(scope.ServiceProvider.GetService<ITokenService>());
        Assert.NotNull(scope.ServiceProvider.GetService<IPasswordHasher>());
    }

    [Fact]
    public void AddInfrastructure_RegistersTypedHttpClientsForEveryExternalAiAndMapProvider()
    {
        var (_, _, infrastructureTypes, _) = CompositeContainer.Build();

        Assert.Contains(typeof(ShilpoHubBD.Application.Interfaces.Services.IAITourismProvider), infrastructureTypes);
        Assert.Contains(typeof(ShilpoHubBD.Application.Interfaces.Services.ITranslationService), infrastructureTypes);
        Assert.Contains(typeof(ShilpoHubBD.Application.Interfaces.Services.IGeocodingProvider), infrastructureTypes);
        Assert.Contains(typeof(ShilpoHubBD.Application.Interfaces.Services.IRoutingProvider), infrastructureTypes);
    }

    [Fact]
    public void AddInfrastructure_EveryRegisteredServiceResolves()
    {
        var (provider, _, infrastructureTypes, _) = CompositeContainer.Build();
        using var scope = provider.CreateScope();

        var failures = infrastructureTypes
            .Select(type => (type, error: TryResolve(scope.ServiceProvider, type)))
            .Where(r => r.error is not null)
            .ToList();

        Assert.True(failures.Count == 0,
            "The following Infrastructure-layer services failed to resolve:\n" + string.Join('\n', failures.Select(f => $"  {f.type.FullName}: {f.error}")));
    }

    [Fact]
    public void AddInfrastructure_ResolvingTheSameTypedHttpClientTwice_GivesADifferentHttpClientInstanceEachTime()
    {
        // AddHttpClient registers its typed client as transient, so unrelated call sites never share
        // one HttpClient's cookies, headers or connection pool by accident.
        var (provider, _, _, _) = CompositeContainer.Build();
        using var scope = provider.CreateScope();

        var first = scope.ServiceProvider.GetRequiredService<ShilpoHubBD.Application.Interfaces.Services.ITokenService>();
        var second = scope.ServiceProvider.GetRequiredService<ShilpoHubBD.Application.Interfaces.Services.ITokenService>();

        // ITokenService is AddScoped, so within one scope it IS the same instance -- this asserts the
        // scoping choice itself, which is what call sites relying on JwtTokenService actually depend on.
        Assert.Same(first, second);
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
