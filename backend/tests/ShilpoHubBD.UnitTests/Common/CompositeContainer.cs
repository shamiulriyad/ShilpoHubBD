using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ShilpoHubBD.Application;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Data;
using ShilpoHubBD.Infrastructure;

namespace ShilpoHubBD.UnitTests.Common;

/// <summary>
/// Builds the same DI graph Program.cs builds (AddApplication + AddData + AddInfrastructure), so the
/// three DependencyInjection tests can check that every ShilpoHubBD service any of the three layers
/// registers actually resolves. No database connection is opened and no HTTP request is made:
/// constructing a DbContext or a typed HttpClient only needs the connection string / base URL to be
/// syntactically valid, not reachable, and every third-party BaseUrl already defaults to a real-looking
/// value when its configuration section is absent, so an otherwise-empty configuration is enough.
/// </summary>
public static class CompositeContainer
{
    /// <summary>The full container plus, for each layer, the ShilpoHubBD service types that layer's
    /// Add* call newly registered (framework/EF Core/HttpClient plumbing added along the way is left out,
    /// since resolving it is not what "every registered interface resolves" is checking).</summary>
    public static (ServiceProvider Provider, IReadOnlyList<Type> DataTypes, IReadOnlyList<Type> InfrastructureTypes, IReadOnlyList<Type> ApplicationTypes) Build()
    {
        var configuration = TestConfiguration.From(
            ("ConnectionStrings:DefaultConnection", "Host=localhost;Database=shilpohub_di_check;Username=postgres;Password=postgres"));
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddProvider(NullLoggerProvider.Instance));
        // A few providers (PgDumpBackupRunner, the Python product-search providers) take IConfiguration
        // directly rather than a typed Options<T>, the same as Program.cs's real container provides it.
        services.AddSingleton(configuration);
        // The SignalR notifiers (IMessageNotifier, ILiveEventNotifier, ILiveClassNotifier) are registered
        // by Program.cs itself, not by AddApplication/AddData/AddInfrastructure, because their SignalR
        // hub types belong to the Api project. Stand-ins here let MessagingService, LiveShoppingService
        // and LiveClassService -- registered by AddApplication -- still resolve in this composite check.
        services.AddSingleton(Substitute.For<IMessageNotifier>());
        services.AddSingleton(Substitute.For<ILiveEventNotifier>());
        services.AddSingleton(Substitute.For<ILiveClassNotifier>());

        var beforeData = OwnTypes(services);
        services.AddData(configuration);
        var dataTypes = OwnTypes(services).Except(beforeData).ToList();

        var beforeInfrastructure = OwnTypes(services);
        services.AddInfrastructure(configuration);
        var infrastructureTypes = OwnTypes(services).Except(beforeInfrastructure).ToList();

        var beforeApplication = OwnTypes(services);
        services.AddApplication(configuration);
        var applicationTypes = OwnTypes(services).Except(beforeApplication).ToList();

        return (services.BuildServiceProvider(), dataTypes, infrastructureTypes, applicationTypes);
    }

    private static HashSet<Type> OwnTypes(IServiceCollection services)
        => services.Select(d => d.ServiceType).Where(t => t.Namespace?.StartsWith("ShilpoHubBD", StringComparison.Ordinal) == true).ToHashSet();
}

file sealed class NullLoggerProvider : ILoggerProvider
{
    public static readonly NullLoggerProvider Instance = new();
    public ILogger CreateLogger(string categoryName) => Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    public void Dispose() { }
}
