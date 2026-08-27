namespace Halaqa.Desktop.Features.Progress.Domain.Entities;

public sealed record CompletedRecitationRange(
    int EditionId,
    int? StartPage,
    int? StartAyahId,
    int? EndPage,
    int? EndAyahId,
    int? EndAyahNumber);

public sealed record StudentLastCompletedProgress(
    CompletedRecitationRange? Memorization,
    CompletedRecitationRange? Review,
    CompletedRecitationRange? Recitation);

public sealed record StudentProgressTotals(
    int TotalSessions,
    int TotalTasks,
    int TotalMistakes,
    int MemorizationTasks,
    int ReviewTasks,
    int RecitationTasks);

public sealed record StudentProgress(
    Guid StudentId,
    StudentLastCompletedProgress LastCompleted,
    StudentProgressTotals Totals);
