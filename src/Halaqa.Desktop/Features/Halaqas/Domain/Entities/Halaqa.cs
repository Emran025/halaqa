namespace Halaqa.Desktop.Features.Halaqas.Domain.Entities;

public enum HalaqaGender
{
    Male,
    Female
}

public enum HalaqaStatus
{
    Active,
    Inactive
}

public sealed record HalaqaTeacher(
    Guid Id,
    string DisplayName,
    string TeacherCode,
    HalaqaGender Gender,
    string Country,
    string City,
    string Qualification,
    int ExperienceYears,
    bool CapacityAvailable);

public sealed record Halaqa(
    Guid Id,
    HalaqaTeacher Teacher,
    string Name,
    string? Description,
    HalaqaStatus Status,
    int StudentCount,
    int? MaxStudents,
    int? AvailableCapacity,
    HalaqaGender Gender,
    string Country,
    string Residence,
    string Timezone,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record HalaqaPage(
    IReadOnlyList<Halaqa> Halaqas,
    int CurrentPage,
    int LastPage,
    int PerPage,
    int Total);

public sealed record CreateHalaqaCommand(
    string Name,
    string? Description,
    HalaqaGender Gender,
    string Country,
    string Residence,
    int? MaxStudents,
    string Timezone,
    HalaqaStatus Status);

public sealed record UpdateHalaqaCommand(
    Guid Id,
    string Name,
    string? Description,
    HalaqaGender Gender,
    string Country,
    string Residence,
    int? MaxStudents,
    string Timezone,
    HalaqaStatus Status);
