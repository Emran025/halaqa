using Halaqa.Desktop.Features.Sessions.Data.Models;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;

namespace Halaqa.Desktop.Features.Sessions.Data.Mappers;

internal static class SessionTaskDirectoryMapper
{
    public static SessionTaskPage ToDomain(SessionTaskCollectionResponseDto dto) => new(
        dto.Tasks.Select(ToDomain).ToArray(),
        dto.Meta.CurrentPage,
        dto.Meta.LastPage,
        dto.Meta.PerPage,
        dto.Meta.Total);

    public static SessionTaskListItem ToDomain(SessionTaskListItemDto dto) => new(
        dto.Id,
        dto.SessionId,
        ParseEnum<SessionTaskType>(dto.TaskType),
        dto.SequenceNo,
        ParseEnum<OfficialSessionTaskState>(dto.State),
        dto.PlannedFromUnitId,
        dto.PlannedToUnitId,
        dto.PlannedAmount,
        dto.ActualAmount,
        dto.Comment,
        dto.Score,
        dto.Gap,
        dto.StartedAt,
        dto.CompletedAt,
        dto.TeacherEvaluation,
        dto.StudentEvaluation,
        dto.MistakesCount);

    private static TEnum ParseEnum<TEnum>(string value) where TEnum : struct, Enum =>
        Enum.Parse<TEnum>(value, ignoreCase: true);
}
