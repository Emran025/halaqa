using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Halaqa.Desktop.Config.Http;

public sealed class SafeBooleanConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Number => reader.TryGetInt64(out var num) ? num != 0 : reader.GetDouble() != 0,
            JsonTokenType.String => bool.TryParse(reader.GetString(), out var b) ? b : reader.GetString() is "1" or "true" or "True",
            JsonTokenType.Null => false,
            _ => throw new JsonException($"Cannot convert {reader.TokenType} to boolean.")
        };
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) =>
        writer.WriteBooleanValue(value);
}

public sealed class SafeNullableBooleanConverter : JsonConverter<bool?>
{
    public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Number => reader.TryGetInt64(out var num) ? num != 0 : reader.GetDouble() != 0,
            JsonTokenType.String => bool.TryParse(reader.GetString(), out var b) ? b : reader.GetString() is "1" or "true" or "True",
            _ => throw new JsonException($"Cannot convert {reader.TokenType} to nullable boolean.")
        };
    }

    public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteBooleanValue(value.Value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
