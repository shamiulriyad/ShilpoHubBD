using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShilpoHubBD.Api.Helpers;

// Browsers post date inputs as "2026-10-10" or "2026-10-10T09:00" (no offset), which System.Text.Json reads as
// DateTimeKind.Unspecified. Npgsql refuses to write Unspecified values to timestamptz columns, so every create /
// update carrying a date failed with a 500. Normalise every inbound DateTime to UTC instead.
public sealed class UtcDateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => Normalise(reader.GetDateTime());

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(Normalise(value));

    internal static DateTime Normalise(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
    };
}

public sealed class UtcNullableDateTimeJsonConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType == JsonTokenType.Null ? null : UtcDateTimeJsonConverter.Normalise(reader.GetDateTime());

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value is null) writer.WriteNullValue();
        else writer.WriteStringValue(UtcDateTimeJsonConverter.Normalise(value.Value));
    }
}
