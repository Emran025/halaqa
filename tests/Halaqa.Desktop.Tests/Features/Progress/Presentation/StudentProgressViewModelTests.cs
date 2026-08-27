using Halaqa.Desktop.Features.Progress.Domain.Entities;
using Halaqa.Desktop.Features.Progress.Domain.Repositories;
using Halaqa.Desktop.Features.Progress.Domain.UseCases;
using Halaqa.Desktop.Features.Progress.Presentation.ViewModels;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Progress.Presentation;

public sealed class StudentProgressViewModelTests
{
    [Fact]
    public async Task Load_DisplaysProgressForCurrentStudentAndSelectedTaskType()
    {
        var repository = new FakeStudentProgressRepository();
        var viewModel = new StudentProgressViewModel(new GetStudentProgressUseCase(repository));
        var studentId = Guid.NewGuid();
        viewModel.Initialize(studentId);
        viewModel.SelectedTaskType = "review";

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal(studentId, repository.StudentId);
        Assert.Equal("review", repository.TaskType);
        Assert.NotNull(viewModel.Progress);
        Assert.Equal(7, viewModel.Progress!.Totals.TotalSessions);
        Assert.False(viewModel.IsError);
    }

    [Fact]
    public async Task Load_WithEmptyStudentIdDoesNotCallRepository()
    {
        var repository = new FakeStudentProgressRepository();
        var viewModel = new StudentProgressViewModel(new GetStudentProgressUseCase(repository));
        viewModel.Initialize(Guid.Empty);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal(Guid.Empty, repository.StudentId);
        Assert.False(viewModel.LoadCommand.CanExecute(null));
    }

    private sealed class FakeStudentProgressRepository : IStudentProgressRepository
    {
        public Guid StudentId { get; private set; }
        public string? TaskType { get; private set; }

        public Task<Result<StudentProgress>> GetAsync(Guid studentId, string? taskType, CancellationToken cancellationToken = default)
        {
            StudentId = studentId;
            TaskType = taskType;
            var range = new CompletedRecitationRange(1, 10, 100, 12, 155, null);
            var progress = new StudentProgress(
                studentId,
                new StudentLastCompletedProgress(range, null, null),
                new StudentProgressTotals(7, 14, 3, 6, 5, 3));
            return Task.FromResult(Result<StudentProgress>.Success(progress));
        }
    }
}
