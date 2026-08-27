using Halaqa.Desktop.Features.Sessions.Data.Models;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;

namespace Halaqa.Desktop.Features.Sessions.Data.Mappers;

internal static class SessionDirectoryMapper
{
    public static SessionPage ToDomain(SessionCollectionResponseDto dto) => new(
        dto.Sessions.Select(ToDomain).ToArray(),
        dto.Meta.CurrentPage,
        dto.Meta.LastPage,
        dto.Meta.PerPage,
        dto.Meta.Total);

    public static SessionListItem ToDomain(SessionListItemDto dto) => new(
        dto.Id,
        dto.HalaqaId,
        ToDomain(dto.Teacher),
        ToDomain(dto.Student),
        dto.FollowUpItemId,
        ParseEnum<SessionTaskType>(dto.TaskType),
        ParseEnum<OfficialSessionState>(dto.State),
        dto.ScheduledAt,
        dto.RequestedAt,
        dto.AcceptedAt,
        dto.ConnectedAt,
        dto.EndedAt,
        dto.EndReason,
        dto.DirectP2POnly,
        dto.CreatedAt,
        dto.UpdatedAt);

    private static SessionParticipant ToDomain(SessionParticipantDto dto) => new(
        dto.Id,
        dto.Role,
        dto.Name,
        dto.Email,
        dto.Phone,
        dto.Status);

    private static TEnum ParseEnum<TEnum>(string value) where TEnum : struct, Enum =>
        Enum.Parse<TEnum>(value, ignoreCase: true);
}
