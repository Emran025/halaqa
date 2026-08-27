namespace Halaqa.Desktop.Features.Evaluations.Domain.Entities;

public enum TaskEvaluatorRole
{
    Teacher,
    Student
}

public sealed record TaskEvaluator(Guid Id, string Name, TaskEvaluatorRole Role);

public sealed record TaskEvaluation(
    decimal Score,
    string? Comment,
    TaskEvaluator Evaluator,
    TaskEvaluatorRole EvaluatorRole,
    DateTimeOffset? UpdatedAt);

public sealed record TaskEvaluationSummary(TaskEvaluation? Teacher, TaskEvaluation? Student);

public sealed record UpsertTaskEvaluationCommand(
    Guid SessionId,
    Guid TaskId,
    decimal Score,
    string? Comment);
