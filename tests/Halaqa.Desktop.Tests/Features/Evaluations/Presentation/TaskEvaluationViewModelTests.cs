using Halaqa.Desktop.Features.Evaluations.Domain.Entities;
using Halaqa.Desktop.Features.Evaluations.Domain.Repositories;
using Halaqa.Desktop.Features.Evaluations.Domain.UseCases;
using Halaqa.Desktop.Features.Evaluations.Presentation.ViewModels;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Evaluations.Presentation;

public sealed class TaskEvaluationViewModelTests
{
    [Fact]
    public async Task Load_DisplaysBothOfficialEvaluationsAndPrefillsCurrentUsersEvaluation()
    {
        var repository = new FakeTaskEvaluationRepository();
        var viewModel = CreateViewModel(repository);
        var sessionId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        viewModel.Initialize(sessionId, taskId, "مهمة اختبار", TaskEvaluatorRole.Student);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal(sessionId, repository.LastSessionId);
        Assert.Equal(taskId, repository.LastTaskId);
        Assert.NotNull(viewModel.TeacherEvaluation);
        Assert.NotNull(viewModel.StudentEvaluation);
        Assert.Equal("72", viewModel.Score);
        Assert.Equal("تقييم ذاتي", viewModel.Comment);
        Assert.False(viewModel.IsError);
    }

    [Fact]
    public async Task Save_SendsScoreAndTrimmedCommentForCurrentParticipant()
    {
        var repository = new FakeTaskEvaluationRepository();
        var viewModel = CreateViewModel(repository);
        var sessionId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        viewModel.Initialize(sessionId, taskId, "مهمة اختبار", TaskEvaluatorRole.Teacher);
        viewModel.Score = "88.5";
        viewModel.Comment = "  أداء جيد  ";

        await viewModel.SaveCommand.ExecuteAsync(null);

        var command = Assert.IsType<UpsertTaskEvaluationCommand>(repository.LastCommand);
        Assert.Equal(sessionId, command.SessionId);
        Assert.Equal(taskId, command.TaskId);
        Assert.Equal(88.5m, command.Score);
        Assert.Equal("أداء جيد", command.Comment);
        Assert.Equal("تم حفظ التقييم من الخادم.", viewModel.Message);
        Assert.False(viewModel.IsError);
    }

    [Fact]
    public async Task Save_WithScoreOutsideContractRangeDoesNotCallRepository()
    {
        var repository = new FakeTaskEvaluationRepository();
        var viewModel = CreateViewModel(repository);
        viewModel.Initialize(Guid.NewGuid(), Guid.NewGuid(), "مهمة اختبار", TaskEvaluatorRole.Teacher);
        viewModel.Score = "101";

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Null(repository.LastCommand);
        Assert.True(viewModel.IsError);
        Assert.Equal("الدرجة يجب أن تكون بين 0 و100.", viewModel.Message);
    }

    private static TaskEvaluationViewModel CreateViewModel(FakeTaskEvaluationRepository repository) =>
        new(new GetTaskEvaluationsUseCase(repository), new UpsertTaskEvaluationUseCase(repository));

    private sealed class FakeTaskEvaluationRepository : ITaskEvaluationRepository
    {
        public Guid LastSessionId { get; private set; }
        public Guid LastTaskId { get; private set; }
        public UpsertTaskEvaluationCommand? LastCommand { get; private set; }

        public Task<Result<TaskEvaluationSummary>> GetAsync(Guid sessionId, Guid taskId, CancellationToken cancellationToken = default)
        {
            LastSessionId = sessionId;
            LastTaskId = taskId;
            return Task.FromResult(Result<TaskEvaluationSummary>.Success(CreateSummary()));
        }

        public Task<Result<TaskEvaluationSummary>> UpsertAsync(UpsertTaskEvaluationCommand command, CancellationToken cancellationToken = default)
        {
            LastCommand = command;
            return Task.FromResult(Result<TaskEvaluationSummary>.Success(CreateSummary()));
        }

        private static TaskEvaluationSummary CreateSummary()
        {
            var teacher = new TaskEvaluator(Guid.NewGuid(), "معلم اختبار", TaskEvaluatorRole.Teacher);
            var student = new TaskEvaluator(Guid.NewGuid(), "طالب اختبار", TaskEvaluatorRole.Student);
            return new TaskEvaluationSummary(
                new TaskEvaluation(91, "تقييم المعلم", teacher, TaskEvaluatorRole.Teacher, DateTimeOffset.UtcNow),
                new TaskEvaluation(72, "تقييم ذاتي", student, TaskEvaluatorRole.Student, DateTimeOffset.UtcNow));
        }
    }
}
