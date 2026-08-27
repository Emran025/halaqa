using Halaqa.Desktop.Features.Evaluations.Data.Models;
using Halaqa.Desktop.Features.Evaluations.Domain.Entities;

namespace Halaqa.Desktop.Features.Evaluations.Data.Mappers;

internal static class TaskEvaluationMapper
{
    public static TaskEvaluationSummary ToDomain(TaskEvaluationResponseDto dto) => new(
        dto.Teacher is null ? null : ToDomain(dto.Teacher),
        dto.Student is null ? null : ToDomain(dto.Student));

    private static TaskEvaluation ToDomain(TaskEvaluationDto dto)
    {
        var role = Enum.Parse<TaskEvaluatorRole>(dto.EvaluatorRole, ignoreCase: true);
        return new TaskEvaluation(dto.Score, dto.Comment, new TaskEvaluator(dto.Evaluator.Id, dto.Evaluator.Name, role), role, dto.UpdatedAt);
    }
}
