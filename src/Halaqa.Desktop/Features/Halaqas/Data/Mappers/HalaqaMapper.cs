using Halaqa.Desktop.Features.Halaqas.Data.Models;
using Halaqa.Desktop.Features.Halaqas.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Halaqas.Data.Mappers;

internal static class HalaqaMapper
{
    public static Result<HalaqaItem> ToDomain(HalaqaDto dto)
    {
        if (dto.Id == Guid.Empty || dto.Teacher is null ||
            string.IsNullOrWhiteSpace(dto.Name) ||
            string.IsNullOrWhiteSpace(dto.Country) ||
            string.IsNullOrWhiteSpace(dto.Residence) ||
            string.IsNullOrWhiteSpace(dto.Timezone) ||
            dto.StudentCount < 0 || dto.MaxStudents is < 0 || dto.AvailableCapacity is < 0 ||
            !TryParse(dto.Gender, out HalaqaGender gender) ||
            !TryParse(dto.Status, out HalaqaStatus status))
        {
            return Result<HalaqaItem>.Failure(UnexpectedResponseError());
        }

        var teacher = ToTeacher(dto.Teacher);
        if (!teacher.IsSuccess || teacher.Value is null)
        {
            return Result<HalaqaItem>.Failure(teacher.Error!);
        }

        return Result<HalaqaItem>.Success(new HalaqaItem(
            dto.Id,
            teacher.Value,
            dto.Name,
            dto.Description,
            status,
            dto.StudentCount,
            dto.MaxStudents,
            dto.AvailableCapacity,
            gender,
            dto.Country,
            dto.Residence,
            dto.Timezone,
            dto.CreatedAt,
            dto.UpdatedAt));
    }

    public static Result<HalaqaPage> ToDomain(HalaqaCollectionResponseDto dto)
    {
        var meta = dto.ResolvedMeta;
        if (dto.Halaqas is null ||
            meta.CurrentPage < 1 || meta.LastPage < 1 || meta.PerPage < 1 || meta.Total < 0)
        {
            return Result<HalaqaPage>.Failure(UnexpectedResponseError());
        }

        var halaqas = dto.Halaqas.Select(ToDomain).ToArray();
        var error = halaqas.Select(result => result.Error).FirstOrDefault(value => value is not null);
        if (error is not null)
        {
            return Result<HalaqaPage>.Failure(error);
        }

        return Result<HalaqaPage>.Success(new HalaqaPage(
            halaqas.Select(result => result.Value!).ToArray(),
            meta.CurrentPage,
            meta.LastPage,
            meta.PerPage,
            meta.Total));
    }

    public static CreateHalaqaRequestDto ToDto(CreateHalaqaCommand command) => new(
        command.Name.Trim(),
        NormalizeOptional(command.Description),
        ToContractValue(command.Gender),
        command.Country.Trim(),
        command.Residence.Trim(),
        command.MaxStudents,
        command.Timezone.Trim(),
        ToContractValue(command.Status));

    public static UpdateHalaqaRequestDto ToDto(UpdateHalaqaCommand command) => new(
        command.Name.Trim(),
        NormalizeOptional(command.Description),
        ToContractValue(command.Gender),
        command.Country.Trim(),
        command.Residence.Trim(),
        command.MaxStudents,
        command.Timezone.Trim(),
        ToContractValue(command.Status));

    private static Result<HalaqaTeacher> ToTeacher(HalaqaTeacherDto dto)
    {
        if (dto.Id == Guid.Empty || string.IsNullOrWhiteSpace(dto.DisplayName) ||
            string.IsNullOrWhiteSpace(dto.TeacherCode) || string.IsNullOrWhiteSpace(dto.Country) ||
            string.IsNullOrWhiteSpace(dto.City) || string.IsNullOrWhiteSpace(dto.Qualification) ||
            dto.ExperienceYears is < 0 or > 80 || !TryParse(dto.Gender, out HalaqaGender gender))
        {
            return Result<HalaqaTeacher>.Failure(UnexpectedResponseError());
        }

        return Result<HalaqaTeacher>.Success(new HalaqaTeacher(
            dto.Id,
            dto.DisplayName,
            dto.TeacherCode,
            gender,
            dto.Country,
            dto.City,
            dto.Qualification,
            dto.ExperienceYears,
            dto.CapacityAvailable));
    }

    private static bool TryParse<TEnum>(string? value, out TEnum parsed) where TEnum : struct, Enum =>
        Enum.TryParse(value, ignoreCase: true, out parsed);

    private static string ToContractValue<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var name = value.ToString();
        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static AppError UnexpectedResponseError() => new(
        AppErrorKind.Unknown,
        "أعاد الخادم بيانات حلقة بصورة غير متوقعة.");
}
