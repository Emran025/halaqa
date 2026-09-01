using Halaqa.Desktop.Features.Memberships.Data.Models;
using Halaqa.Desktop.Features.Memberships.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Memberships.Data.Mappers;

internal static class HalaqaMembershipMapper
{
    public static Result<HalaqaMembership> ToDomain(HalaqaMembershipDto dto)
    {
        if (dto.Id == Guid.Empty || dto.HalaqaId == Guid.Empty || dto.Student is null ||
            dto.JoinedAt == default || !TryParse(dto.Status, out MembershipStatus status))
        {
            return Result<HalaqaMembership>.Failure(UnexpectedResponseError());
        }

        var student = ToStudent(dto.Student);
        if (!student.IsSuccess || student.Value is null)
        {
            return Result<HalaqaMembership>.Failure(student.Error!);
        }

        return Result<HalaqaMembership>.Success(new HalaqaMembership(
            dto.Id,
            dto.HalaqaId,
            student.Value,
            status,
            dto.JoinedAt));
    }

    public static Result<MembershipPage> ToDomain(MembershipCollectionResponseDto dto)
    {
        var meta = dto.ResolvedMeta;
        if (dto.Memberships is null ||
            meta.CurrentPage < 1 || meta.LastPage < 1 || meta.PerPage < 1 || meta.Total < 0)
        {
            return Result<MembershipPage>.Failure(UnexpectedResponseError());
        }

        var memberships = dto.Memberships.Select(ToDomain).ToArray();
        var error = memberships.Select(result => result.Error).FirstOrDefault(value => value is not null);
        if (error is not null)
        {
            return Result<MembershipPage>.Failure(error);
        }

        return Result<MembershipPage>.Success(new MembershipPage(
            memberships.Select(result => result.Value!).ToArray(),
            meta.CurrentPage,
            meta.LastPage,
            meta.PerPage,
            meta.Total));
    }

    public static AssignStudentRequestDto ToDto(AssignStudentToHalaqaCommand command) => new(command.StudentId);

    public static UpdateMembershipRequestDto ToDto(UpdateHalaqaMembershipCommand command) => new(
        ToContractValue(command.Status),
        NormalizeOptional(command.Reason));

    private static Result<MembershipStudent> ToStudent(MembershipStudentDto dto)
    {
        if (dto.Id == Guid.Empty ||
            !string.Equals(dto.Role, "student", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(dto.Name) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Status))
        {
            return Result<MembershipStudent>.Failure(UnexpectedResponseError());
        }

        return Result<MembershipStudent>.Success(new MembershipStudent(
            dto.Id,
            dto.Name,
            dto.Email,
            dto.Phone,
            dto.Status,
            dto.CreatedAt,
            dto.UpdatedAt));
    }

    private static bool TryParse(string? value, out MembershipStatus parsed) =>
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
        "أعاد الخادم بيانات عضوية بصورة غير متوقعة.");
}
