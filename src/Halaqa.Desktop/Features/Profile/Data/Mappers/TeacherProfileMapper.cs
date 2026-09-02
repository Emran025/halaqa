using Halaqa.Desktop.Features.Profile.Data.Models;
using Halaqa.Desktop.Features.Profile.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Profile.Data.Mappers;

internal static class TeacherProfileMapper
{
    public static Result<TeacherProfile> ToDomain(TeacherProfileDto dto)
    {
        if (dto.Id == Guid.Empty ||
            string.IsNullOrWhiteSpace(dto.DisplayName) ||
            string.IsNullOrWhiteSpace(dto.TeacherCode) ||
            string.IsNullOrWhiteSpace(dto.Country) ||
            string.IsNullOrWhiteSpace(dto.City) ||
            dto.ExperienceYears is < 0 or > 80 ||
            dto.ActiveHalaqaCount < 0 ||
            !TryParse(dto.Gender, out TeacherGender gender))
        {
            return UnexpectedResponse();
        }

        var halaqas = (dto.PublicHalaqas ?? Array.Empty<TeacherHalaqaSummaryDto>())
            .Select(ToHalaqa)
            .ToArray();
        var halaqaError = halaqas.Select(result => result.Error).FirstOrDefault(error => error is not null);
        if (halaqaError is not null)
        {
            return Result<TeacherProfile>.Failure(halaqaError);
        }

        var documents = (dto.Documents ?? Array.Empty<TeacherDocumentSummaryDto>())
            .Select(ToDocument)
            .ToArray();
        var documentError = documents.Select(result => result.Error).FirstOrDefault(error => error is not null);
        if (documentError is not null)
        {
            return Result<TeacherProfile>.Failure(documentError);
        }

        return Result<TeacherProfile>.Success(new TeacherProfile(
            dto.Id,
            dto.DisplayName,
            dto.TeacherCode,
            dto.Avatar,
            gender,
            dto.Country,
            dto.City,
            dto.Qualification,
            dto.ExperienceYears,
            dto.CapacityAvailable,
            dto.Bio,
            dto.ActiveHalaqaCount,
            halaqas.Select(result => result.Value!).ToArray(),
            dto.BirthDate,
            dto.Email,
            dto.Phone,
            dto.PhoneZone,
            dto.WhatsappPhone,
            dto.WhatsappZone,
            dto.Residence,
            dto.AvailableTime,
            documents.Select(result => result.Value!).ToArray()));
    }

    public static UpdateTeacherProfileRequestDto ToDto(UpdateTeacherProfileCommand command) => new(
        command.Name.IsSpecified,
        command.Name.Value,
        command.BirthDate.IsSpecified,
        command.BirthDate.Value,
        command.Gender.IsSpecified,
        command.Gender.Value is { } gender ? ToContractValue(gender) : null,
        command.Country.IsSpecified,
        command.Country.Value,
        command.City.IsSpecified,
        command.City.Value,
        command.Residence.IsSpecified,
        command.Residence.Value,
        command.Phone.IsSpecified,
        command.Phone.Value,
        command.PhoneZone.IsSpecified,
        command.PhoneZone.Value,
        command.WhatsappPhone.IsSpecified,
        command.WhatsappPhone.Value,
        command.WhatsappZone.IsSpecified,
        command.WhatsappZone.Value,
        command.Qualification.IsSpecified,
        command.Qualification.Value,
        command.ExperienceYears.IsSpecified,
        command.ExperienceYears.Value,
        command.AvailableTime.IsSpecified,
        command.AvailableTime.Value,
        command.Bio.IsSpecified,
        command.Bio.Value,
        command.MaxHalaqas.IsSpecified,
        command.MaxHalaqas.Value);

    private static Result<TeacherHalaqaSummary> ToHalaqa(TeacherHalaqaSummaryDto dto)
    {
        if (dto.Id == Guid.Empty ||
            string.IsNullOrWhiteSpace(dto.Name) ||
            string.IsNullOrWhiteSpace(dto.Status) ||
            string.IsNullOrWhiteSpace(dto.Country) ||
            string.IsNullOrWhiteSpace(dto.Residence) ||
            !TryParse(dto.Gender, out TeacherGender gender))
        {
            return UnexpectedResponse<TeacherHalaqaSummary>();
        }

        return Result<TeacherHalaqaSummary>.Success(new TeacherHalaqaSummary(
            dto.Id,
            dto.Name,
            dto.Status,
            gender,
            dto.Country,
            dto.Residence,
            dto.AvailableCapacity));
    }

    private static Result<TeacherDocumentSummary> ToDocument(TeacherDocumentSummaryDto dto)
    {
        if (dto.Id <= 0 ||
            string.IsNullOrWhiteSpace(dto.Name) ||
            string.IsNullOrWhiteSpace(dto.CertificateType))
        {
            return UnexpectedResponse<TeacherDocumentSummary>();
        }

        return Result<TeacherDocumentSummary>.Success(new TeacherDocumentSummary(
            dto.Id,
            dto.Name,
            dto.CertificateType,
            dto.CertificateTypeOther,
            dto.Riwayah,
            dto.IssuingPlace,
            dto.IssuingDate,
            dto.FileUrl,
            dto.HasFile));
    }

    private static bool TryParse<TEnum>(string? value, out TEnum parsed) where TEnum : struct, Enum =>
        Enum.TryParse(value, ignoreCase: true, out parsed);

    private static string ToContractValue<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var name = value.ToString();
        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    private static Result<TeacherProfile> UnexpectedResponse() =>
        Result<TeacherProfile>.Failure(CreateUnexpectedResponseError());

    private static Result<T> UnexpectedResponse<T>() =>
        Result<T>.Failure(CreateUnexpectedResponseError());

    private static AppError CreateUnexpectedResponseError() => new(
        AppErrorKind.Unknown,
        "أعاد الخادم بيانات ملف المعلم بصورة غير متوقعة.");
}
