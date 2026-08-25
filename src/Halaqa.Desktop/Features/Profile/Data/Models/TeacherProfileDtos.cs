using System.Text.Json;
using System.Text.Json.Serialization;

namespace Halaqa.Desktop.Features.Profile.Data.Models;

internal sealed record TeacherProfileResponseDto(
    [property: JsonPropertyName("teacher_profile")] TeacherProfileDto TeacherProfile);

internal sealed record TeacherProfileDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("display_name")] string DisplayName,
    [property: JsonPropertyName("teacher_code")] string TeacherCode,
    [property: JsonPropertyName("avatar")] string? Avatar,
    [property: JsonPropertyName("gender")] string Gender,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("qualification")] string? Qualification,
    [property: JsonPropertyName("experience_years")] int? ExperienceYears,
    [property: JsonPropertyName("capacity_available")] bool CapacityAvailable,
    [property: JsonPropertyName("bio")] string? Bio,
    [property: JsonPropertyName("active_halaqa_count")] int ActiveHalaqaCount,
    [property: JsonPropertyName("public_halaqas")] IReadOnlyList<TeacherHalaqaSummaryDto>? PublicHalaqas,
    [property: JsonPropertyName("birth_date")] DateOnly? BirthDate,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("phone")] string? Phone,
    [property: JsonPropertyName("phone_zone")] string? PhoneZone,
    [property: JsonPropertyName("whatsapp_phone")] string? WhatsappPhone,
    [property: JsonPropertyName("whatsapp_zone")] string? WhatsappZone,
    [property: JsonPropertyName("residence")] string? Residence,
    [property: JsonPropertyName("available_time")] string? AvailableTime,
    [property: JsonPropertyName("documents")] IReadOnlyList<TeacherDocumentSummaryDto>? Documents);

internal sealed record TeacherHalaqaSummaryDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("gender")] string Gender,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("residence")] string Residence,
    [property: JsonPropertyName("available_capacity")] int? AvailableCapacity);

internal sealed record TeacherDocumentSummaryDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("certificate_type")] string CertificateType,
    [property: JsonPropertyName("certificate_type_other")] string? CertificateTypeOther,
    [property: JsonPropertyName("riwayah")] string? Riwayah,
    [property: JsonPropertyName("issuing_place")] string? IssuingPlace,
    [property: JsonPropertyName("issuing_date")] DateOnly? IssuingDate,
    [property: JsonPropertyName("file_url")] string? FileUrl,
    [property: JsonPropertyName("has_file")] bool HasFile);

[JsonConverter(typeof(UpdateTeacherProfileRequestDtoJsonConverter))]
internal sealed record UpdateTeacherProfileRequestDto(
    bool IncludeName,
    string? Name,
    bool IncludeBirthDate,
    DateOnly? BirthDate,
    bool IncludeGender,
    string? Gender,
    bool IncludeCountry,
    string? Country,
    bool IncludeCity,
    string? City,
    bool IncludeResidence,
    string? Residence,
    bool IncludePhone,
    string? Phone,
    bool IncludePhoneZone,
    string? PhoneZone,
    bool IncludeWhatsappPhone,
    string? WhatsappPhone,
    bool IncludeWhatsappZone,
    string? WhatsappZone,
    bool IncludeQualification,
    string? Qualification,
    bool IncludeExperienceYears,
    int? ExperienceYears,
    bool IncludeAvailableTime,
    string? AvailableTime,
    bool IncludeBio,
    string? Bio,
    bool IncludeMaxHalaqas,
    int? MaxHalaqas);

internal sealed class UpdateTeacherProfileRequestDtoJsonConverter : JsonConverter<UpdateTeacherProfileRequestDto>
{
    public override UpdateTeacherProfileRequestDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotSupportedException("طلبات تحديث ملف المعلم تُسلسل فقط ولا تُقرأ من الخادم.");

    public override void Write(Utf8JsonWriter writer, UpdateTeacherProfileRequestDto value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        WriteOptional(writer, "name", value.IncludeName, value.Name, options);
        WriteOptional(writer, "birth_date", value.IncludeBirthDate, value.BirthDate, options);
        WriteOptional(writer, "gender", value.IncludeGender, value.Gender, options);
        WriteOptional(writer, "country", value.IncludeCountry, value.Country, options);
        WriteOptional(writer, "city", value.IncludeCity, value.City, options);
        WriteOptional(writer, "residence", value.IncludeResidence, value.Residence, options);
        WriteOptional(writer, "phone", value.IncludePhone, value.Phone, options);
        WriteOptional(writer, "phone_zone", value.IncludePhoneZone, value.PhoneZone, options);
        WriteOptional(writer, "whatsapp_phone", value.IncludeWhatsappPhone, value.WhatsappPhone, options);
        WriteOptional(writer, "whatsapp_zone", value.IncludeWhatsappZone, value.WhatsappZone, options);
        WriteOptional(writer, "qualification", value.IncludeQualification, value.Qualification, options);
        WriteOptional(writer, "experience_years", value.IncludeExperienceYears, value.ExperienceYears, options);
        WriteOptional(writer, "available_time", value.IncludeAvailableTime, value.AvailableTime, options);
        WriteOptional(writer, "bio", value.IncludeBio, value.Bio, options);
        WriteOptional(writer, "max_halaqas", value.IncludeMaxHalaqas, value.MaxHalaqas, options);
        writer.WriteEndObject();
    }

    private static void WriteOptional<T>(Utf8JsonWriter writer, string propertyName, bool include, T? value, JsonSerializerOptions options)
    {
        if (!include)
        {
            return;
        }

        writer.WritePropertyName(propertyName);
        JsonSerializer.Serialize(writer, value, options);
    }
}
