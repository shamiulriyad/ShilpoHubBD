using System.Text.Json;
using ShilpoHubBD.Api.Helpers;

namespace ShilpoHubBD.UnitTests.Features.Platform.Json;

[Trait("Feature", "Platform")]
[Trait("Layer", "JSON converter")]
public class UtcNullableDateTimeJsonConverterTests
{
    private static readonly JsonSerializerOptions Options = new() { Converters = { new UtcNullableDateTimeJsonConverter() } };

    private sealed record Wrapper(DateTime? Value);

    [Fact]
    public void Read_NullToken_ReturnsNull()
    {
        var result = JsonSerializer.Deserialize<Wrapper>("""{"Value":null}""", Options)!;

        Assert.Null(result.Value);
    }

    [Fact]
    public void Read_OffsetLessDateTimeString_IsTreatedAsUtc()
    {
        var result = JsonSerializer.Deserialize<Wrapper>("""{"Value":"2026-10-10T09:00:00"}""", Options)!;

        Assert.Equal(DateTimeKind.Utc, result.Value!.Value.Kind);
        Assert.Equal(new DateTime(2026, 10, 10, 9, 0, 0, DateTimeKind.Utc), result.Value);
    }

    [Fact]
    public void Read_StringWithAnOffset_IsConvertedToTheEquivalentUtcInstant()
    {
        var result = JsonSerializer.Deserialize<Wrapper>("""{"Value":"2026-10-10T15:00:00+06:00"}""", Options)!;

        Assert.Equal(new DateTime(2026, 10, 10, 9, 0, 0, DateTimeKind.Utc), result.Value);
    }

    [Fact]
    public void Write_Null_WritesJsonNull()
    {
        var json = JsonSerializer.Serialize(new Wrapper(null), Options);

        Assert.Contains("\"Value\":null", json);
    }

    [Fact]
    public void Write_UnspecifiedValue_IsWrittenAsIfItWereUtc()
    {
        var value = new Wrapper(new DateTime(2026, 10, 10, 9, 0, 0, DateTimeKind.Unspecified));

        var json = JsonSerializer.Serialize(value, Options);

        Assert.Contains("2026-10-10T09:00:00Z", json);
    }

    [Fact]
    public void Write_UtcValue_RoundTripsThroughRead()
    {
        var original = new Wrapper(new DateTime(2026, 10, 10, 9, 0, 0, DateTimeKind.Utc));

        var json = JsonSerializer.Serialize(original, Options);
        var roundTripped = JsonSerializer.Deserialize<Wrapper>(json, Options);

        Assert.Equal(original.Value, roundTripped!.Value);
    }
}
