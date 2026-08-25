using System.Text.Json.Serialization;

namespace Halaqa.Desktop.Features.Halaqas.Data.Models;

internal sealed record HalaqaTeacherDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("display_name")] string DisplayName,
    [property: JsonPropertyName("teacher_code")] string TeacherCode,
    [property: JsonPropertyName("gender")] string Gender,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("qualification")] string Qualification,
    [property: JsonPropertyName("experience_years")] int ExperienceYears,
    [property: JsonPropertyName("capacity_available")] bool CapacityAvailable);

internal sealed record HalaqaDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("teacher")] HalaqaTeacherDto Teacher,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("student_count")] int StudentCount,
    [property: JsonPropertyName("max_students")] int? MaxStudents,
    [property: JsonPropertyName("available_capacity")] int? AvailableCapacity,
    [property: JsonPropertyName("gender")] string Gender,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("residence")] string Residence,
    [property: JsonPropertyName("timezone")] string Timezone,
    [property: JsonPropertyName("created_at")] DateTimeOffset? CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset? UpdatedAt);

internal sealed record HalaqaResponseDto(
    [property: JsonPropertyName("halaqa")] HalaqaDto Halaqa);

internal sealed record HalaqaCollectionResponseDto(
    [property: JsonPropertyName("halaqas")] IReadOnlyList<HalaqaDto> Halaqas,
    [property: JsonPropertyName("meta")] HalaqaPaginationMetaDto Meta);

internal sealed record HalaqaPaginationMetaDto(
    [property: JsonPropertyName("current_page")] int CurrentPage,
    [property: JsonPropertyName("last_page")] int LastPage,
    [property: JsonPropertyName("per_page")] int PerPage,
    [property: JsonPropertyName("total")] int Total);

internal sealed record CreateHalaqaRequestDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("gender")] string Gender,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("residence")] string Residence,
    [property: JsonPropertyName("max_students")] int? MaxStudents,
    [property: JsonPropertyName("timezone")] string Timezone,
    [property: JsonPropertyName("status")] string Status);

internal sealed record UpdateHalaqaRequestDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("gender")] string Gender,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("residence")] string Residence,
    [property: JsonPropertyName("max_students")] int? MaxStudents,
    [property: JsonPropertyName("timezone")] string Timezone,
    [property: JsonPropertyName("status")] string Status);
