using Halaqa.Desktop.Features.Auth.Domain.Entities;
using Halaqa.Desktop.Features.Profile.Data.Models;
using Halaqa.Desktop.Features.Profile.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Profile.Data.Mappers;

internal static class ProfileMapper
{
    public static Result<UserProfile> ToDomain(UserProfileDto dto)
    {
        if (dto.Id == Guid.Empty ||
            string.IsNullOrWhiteSpace(dto.Name) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Status) ||
            !Enum.TryParse<UserRole>(dto.Role, ignoreCase: true, out var role))
        {
            return Result<UserProfile>.Failure(new AppError(
                AppErrorKind.Unknown,
                "أعاد الخادم بيانات حساب غير متوقعة."));
        }

        return Result<UserProfile>.Success(new UserProfile(
            dto.Id,
            role,
            dto.Name,
            dto.Email,
            dto.Phone,
            dto.Status));
    }

    public static UpdateUserProfileRequestDto ToDto(UpdateUserProfileCommand command) => new(
        command.Name.IsSpecified,
        command.Name.Value,
        command.Phone.IsSpecified,
        command.Phone.Value,
        command.MemorizationLevel.IsSpecified,
        command.MemorizationLevel.Value,
        command.ReviewLevel.IsSpecified,
        command.ReviewLevel.Value);
}
