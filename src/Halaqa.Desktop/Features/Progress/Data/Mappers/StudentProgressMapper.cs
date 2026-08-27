using Halaqa.Desktop.Features.Progress.Data.Models;
using Halaqa.Desktop.Features.Progress.Domain.Entities;

namespace Halaqa.Desktop.Features.Progress.Data.Mappers;

internal static class StudentProgressMapper
{
    public static StudentProgress ToDomain(StudentProgressDto dto) => new(
        dto.StudentId,
        new StudentLastCompletedProgress(
            ToDomain(dto.LastCompleted.Memorization),
            ToDomain(dto.LastCompleted.Review),
            ToDomain(dto.LastCompleted.Recitation)),
        new StudentProgressTotals(
            dto.Totals.TotalSessions,
            dto.Totals.TotalTasks,
            dto.Totals.TotalMistakes,
            dto.Totals.MemorizationTasks,
            dto.Totals.ReviewTasks,
            dto.Totals.RecitationTasks));

    private static CompletedRecitationRange? ToDomain(RecitationRangeDto? dto) =>
        dto is null ? null : new CompletedRecitationRange(dto.EditionId, dto.StartPage, dto.StartAyahId, dto.EndPage, dto.EndAyahId, dto.EndAyahNumber);
}
