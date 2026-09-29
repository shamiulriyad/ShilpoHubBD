using Microsoft.Extensions.Configuration;

namespace ShilpoHubBD.UnitTests.Common;

public static class TestConfiguration
{
    public static IConfiguration From(params (string Key, string? Value)[] values)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>(v.Key, v.Value)))
            .Build();

    public static IConfiguration Empty() => From();
}
