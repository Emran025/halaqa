using System.Text.Json;
using System.Text.Json.Serialization;

namespace Halaqa.Desktop.Features.Profile.Data.Models;

internal sealed record UserProfileResponseDto([property: JsonPropertyName("user")] UserProfileDto User);

internal sealed record UserProfileDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("phone")] string? Phone,
    [property: JsonPropertyName("status")] string Status);

[JsonConverter(typeof(UpdateUserProfileRequestDtoJsonConverter))]
internal sealed record UpdateUserProfileRequestDto(
    bool IncludeName,
    string? Name,
    bool IncludePhone,
    string? Phone,
    bool IncludeMemorizationLevel,
    string? MemorizationLevel,
    bool IncludeReviewLevel,
    string? ReviewLevel);

internal sealed class UpdateUserProfileRequestDtoJsonConverter : JsonConverter<UpdateUserProfileRequestDto>
{
    public override UpdateUserProfileRequestDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotSupportedException("طلبات تحديث الملف تُسلسل فقط ولا تُقرأ من الخادم.");

    public override void Write(Utf8JsonWriter writer, UpdateUserProfileRequestDto value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        WriteOptionalString(writer, "name", value.IncludeName, value.Name);
        WriteOptionalString(writer, "phone", value.IncludePhone, value.Phone);
        WriteOptionalString(writer, "memorization_level", value.IncludeMemorizationLevel, value.MemorizationLevel);
        WriteOptionalString(writer, "review_level", value.IncludeReviewLevel, value.ReviewLevel);
        writer.WriteEndObject();
    }

    private static void WriteOptionalString(Utf8JsonWriter writer, string propertyName, bool includeProperty, string? propertyValue)
    {
        if (!includeProperty)
        {
            return;
        }

        writer.WritePropertyName(propertyName);
        if (propertyValue is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(propertyValue);
    }
}
