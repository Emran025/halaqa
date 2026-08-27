using System.Text.Json.Serialization;

namespace Halaqa.Desktop.Features.Evaluations.Data.Models;

internal sealed record UpsertTaskEvaluationRequestDto(
    [property: JsonPropertyName("score")] decimal Score,
    [property: JsonPropertyName("comment"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Comment);

internal sealed record TaskEvaluationUserDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name);

internal sealed record TaskEvaluationDto(
    [property: JsonPropertyName("score")] decimal Score,
    [property: JsonPropertyName("comment")] string? Comment,
    [property: JsonPropertyName("evaluator")] TaskEvaluationUserDto Evaluator,
    [property: JsonPropertyName("evaluator_role")] string EvaluatorRole,
    [property: JsonPropertyName("updated_at")] DateTimeOffset? UpdatedAt);

internal sealed record TaskEvaluationResponseDto(
    [property: JsonPropertyName("teacher")] TaskEvaluationDto? Teacher,
    [property: JsonPropertyName("student")] TaskEvaluationDto? Student);
