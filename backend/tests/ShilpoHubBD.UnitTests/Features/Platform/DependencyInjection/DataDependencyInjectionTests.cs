using Microsoft.Extensions.DependencyInjection;
using ShilpoHubBD.Data;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Platform.DependencyInjection;

[Trait("Feature", "Platform")]
[Trait("Layer", "DI registration")]
public class DataDependencyInjectionTests
{
    [Fact]
    public void AddData_RegistersShilpoHubDbContext()
    {
        var (provider, dataTypes, _, _) = CompositeContainer.Build();
        using var scope = provider.CreateScope();

        Assert.Contains(typeof(ShilpoHubDbContext), dataTypes);
        Assert.NotNull(scope.ServiceProvider.GetService<ShilpoHubDbContext>());
    }

    [Fact]
    public void AddData_RegistersMoreThanOneHundredRepositories()
    {
        var (_, dataTypes, _, _) = CompositeContainer.Build();

        var repositories = dataTypes.Where(t => t.Name.EndsWith("Repository", StringComparison.Ordinal)).ToList();

        Assert.True(repositories.Count > 100, $"Expected over 100 repository registrations, found {repositories.Count}.");
    }

    [Fact]
    public void AddData_EveryRegisteredServiceResolves()
    {
        var (provider, dataTypes, _, _) = CompositeContainer.Build();
        using var scope = provider.CreateScope();

        var failures = dataTypes
            .Select(type => (type, error: TryResolve(scope.ServiceProvider, type)))
            .Where(r => r.error is not null)
            .ToList();

        Assert.True(failures.Count == 0,
            "The following Data-layer services failed to resolve:\n" + string.Join('\n', failures.Select(f => $"  {f.type.FullName}: {f.error}")));
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
