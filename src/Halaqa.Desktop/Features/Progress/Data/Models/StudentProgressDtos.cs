using System.Text.Json.Serialization;

namespace Halaqa.Desktop.Features.Progress.Data.Models;

internal sealed record RecitationRangeDto(
    [property: JsonPropertyName("edition_id")] int EditionId,
    [property: JsonPropertyName("start_page")] int? StartPage,
    [property: JsonPropertyName("start_ayah_id")] int? StartAyahId,
    [property: JsonPropertyName("end_page")] int? EndPage,
    [property: JsonPropertyName("end_ayah_id")] int? EndAyahId,
    [property: JsonPropertyName("end_ayah_number")] int? EndAyahNumber);

internal sealed record LastCompletedProgressDto(
    [property: JsonPropertyName("memorization")] RecitationRangeDto? Memorization,
    [property: JsonPropertyName("review")] RecitationRangeDto? Review,
    [property: JsonPropertyName("recitation")] RecitationRangeDto? Recitation);

internal sealed record ProgressTotalsDto(
    [property: JsonPropertyName("total_sessions")] int TotalSessions,
    [property: JsonPropertyName("total_tasks")] int TotalTasks,
    [property: JsonPropertyName("total_mistakes")] int TotalMistakes,
    [property: JsonPropertyName("memorization_tasks")] int MemorizationTasks,
    [property: JsonPropertyName("review_tasks")] int ReviewTasks,
    [property: JsonPropertyName("recitation_tasks")] int RecitationTasks);

internal sealed record StudentProgressDto(
    [property: JsonPropertyName("student_id")] Guid StudentId,
    [property: JsonPropertyName("last_completed")] LastCompletedProgressDto LastCompleted,
    [property: JsonPropertyName("totals")] ProgressTotalsDto Totals);

internal sealed record StudentProgressResponseDto([property: JsonPropertyName("progress")] StudentProgressDto Progress);
