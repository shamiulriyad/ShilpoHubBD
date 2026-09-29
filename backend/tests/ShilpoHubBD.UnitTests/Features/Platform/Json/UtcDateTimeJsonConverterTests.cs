using System.Text.Json;
using ShilpoHubBD.Api.Helpers;

namespace ShilpoHubBD.UnitTests.Features.Platform.Json;

[Trait("Feature", "Platform")]
[Trait("Layer", "JSON converter")]
public class UtcDateTimeJsonConverterTests
{
    private static readonly JsonSerializerOptions Options = new() { Converters = { new UtcDateTimeJsonConverter() } };

    private sealed record Wrapper(DateTime Value);

    [Fact]
    public void Read_OffsetLessDateTimeString_IsTreatedAsUtc()
    {
        var result = JsonSerializer.Deserialize<Wrapper>("""{"Value":"2026-10-10T09:00:00"}""", Options)!;

        Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
        Assert.Equal(new DateTime(2026, 10, 10, 9, 0, 0, DateTimeKind.Utc), result.Value);
    }

    [Fact]
    public void Read_DateOnlyString_IsTreatedAsMidnightUtc()
    {
        var result = JsonSerializer.Deserialize<Wrapper>("""{"Value":"2026-10-10"}""", Options)!;

        Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
        Assert.Equal(new DateTime(2026, 10, 10, 0, 0, 0, DateTimeKind.Utc), result.Value);
    }

    [Fact]
    public void Read_StringWithAZOffset_KeepsItsUtcInstant()
    {
        var result = JsonSerializer.Deserialize<Wrapper>("""{"Value":"2026-10-10T09:00:00Z"}""", Options)!;

        Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
        Assert.Equal(new DateTime(2026, 10, 10, 9, 0, 0, DateTimeKind.Utc), result.Value);
    }

    [Fact]
    public void Read_StringWithAPositiveOffset_IsConvertedToTheEquivalentUtcInstant()
    {
        // +06:00 is Bangladesh Standard Time, the app's home timezone -- 15:00+06:00 is 09:00 UTC.
        var result = JsonSerializer.Deserialize<Wrapper>("""{"Value":"2026-10-10T15:00:00+06:00"}""", Options)!;

        Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
        Assert.Equal(new DateTime(2026, 10, 10, 9, 0, 0, DateTimeKind.Utc), result.Value);
    }

    [Fact]
    public void Write_UtcValue_RoundTripsThroughRead()
    {
        var original = new Wrapper(new DateTime(2026, 10, 10, 9, 0, 0, DateTimeKind.Utc));

        var json = JsonSerializer.Serialize(original, Options);
        var roundTripped = JsonSerializer.Deserialize<Wrapper>(json, Options);

        Assert.Equal(original.Value, roundTripped!.Value);
    }

    [Fact]
    public void Write_LocalValue_ConvertsToItsUtcInstant()
    {
        var local = new DateTime(2026, 10, 10, 9, 0, 0, DateTimeKind.Local);

        var json = JsonSerializer.Serialize(new Wrapper(local), Options);

        Assert.Contains(local.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"), json);
    }

    [Fact]
    public void Write_UnspecifiedValue_IsWrittenAsIfItWereUtc()
    {
        var value = new Wrapper(new DateTime(2026, 10, 10, 9, 0, 0, DateTimeKind.Unspecified));

        var json = JsonSerializer.Serialize(value, Options);

        Assert.Contains("2026-10-10T09:00:00Z", json);
    }
}
