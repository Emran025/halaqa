using System.Text.Json.Serialization;

namespace Halaqa.Desktop.Features.Sessions.Data.Models;

internal sealed record CreateSessionTaskRequestDto(
    [property: JsonPropertyName("task_type")] string TaskType,
    [property: JsonPropertyName("client_operation_id")] Guid ClientOperationId,
    [property: JsonPropertyName("sequence_no"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? SequenceNo,
    [property: JsonPropertyName("planned_amount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? PlannedAmount,
    [property: JsonPropertyName("planned_from_unit_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? PlannedFromUnitId,
    [property: JsonPropertyName("planned_to_unit_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? PlannedToUnitId,
    [property: JsonPropertyName("start_page"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? StartPage,
    [property: JsonPropertyName("start_ayah_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? StartAyahId,
    [property: JsonPropertyName("end_page"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? EndPage,
    [property: JsonPropertyName("end_ayah_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? EndAyahId);

internal sealed record SaveSessionTaskDraftRequestDto(
    [property: JsonPropertyName("client_operation_id")] Guid ClientOperationId,
    [property: JsonPropertyName("current_page"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? CurrentPage,
    [property: JsonPropertyName("current_ayah_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? CurrentAyahId);

internal sealed record UpdateSessionTaskRequestDto(
    [property: JsonPropertyName("planned_from_unit_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? PlannedFromUnitId,
    [property: JsonPropertyName("planned_to_unit_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? PlannedToUnitId,
    [property: JsonPropertyName("start_page"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? StartPage,
    [property: JsonPropertyName("start_ayah_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? StartAyahId,
    [property: JsonPropertyName("end_page"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? EndPage,
    [property: JsonPropertyName("end_ayah_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? EndAyahId,
    [property: JsonPropertyName("current_page"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? CurrentPage,
    [property: JsonPropertyName("current_ayah_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? CurrentAyahId,
    [property: JsonPropertyName("state"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? State,
    [property: JsonPropertyName("planned_amount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? PlannedAmount,
    [property: JsonPropertyName("actual_amount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? ActualAmount);

internal sealed record SessionTaskResponseDto(
    [property: JsonPropertyName("task")] SessionTaskListItemDto Task);

internal sealed record SessionTaskListItemDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("session_id")] Guid SessionId,
    [property: JsonPropertyName("task_type")] string TaskType,
    [property: JsonPropertyName("sequence_no")] int SequenceNo,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("planned_from_unit_id")] int? PlannedFromUnitId,
    [property: JsonPropertyName("planned_to_unit_id")] int? PlannedToUnitId,
    [property: JsonPropertyName("planned_amount")] decimal? PlannedAmount,
    [property: JsonPropertyName("actual_amount")] decimal? ActualAmount,
    [property: JsonPropertyName("comment")] string? Comment,
    [property: JsonPropertyName("score")] int? Score,
    [property: JsonPropertyName("gap")] decimal? Gap,
    [property: JsonPropertyName("started_at")] DateTimeOffset? StartedAt,
    [property: JsonPropertyName("completed_at")] DateTimeOffset? CompletedAt,
    [property: JsonPropertyName("teacher_evaluation")] decimal? TeacherEvaluation,
    [property: JsonPropertyName("student_evaluation")] decimal? StudentEvaluation,
    [property: JsonPropertyName("mistakes_count")] int MistakesCount);

internal sealed record SessionTaskPaginationMetaDto(
    [property: JsonPropertyName("current_page")] int CurrentPage,
    [property: JsonPropertyName("last_page")] int LastPage,
    [property: JsonPropertyName("per_page")] int PerPage,
    [property: JsonPropertyName("total")] int Total);

internal sealed record SessionTaskCollectionResponseDto(
    [property: JsonPropertyName("tasks")] IReadOnlyList<SessionTaskListItemDto> Tasks,
    [property: JsonPropertyName("meta")] SessionTaskPaginationMetaDto Meta);
