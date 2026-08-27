using Halaqa.Desktop.Features.Mistakes.Domain.Entities;
using Halaqa.Desktop.Features.Mistakes.Domain.Repositories;
using Halaqa.Desktop.Features.Mistakes.Domain.UseCases;
using Halaqa.Desktop.Features.Mistakes.Presentation.ViewModels;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Mistakes.Presentation;

public sealed class MistakeReportViewModelTests
{
    [Fact]
    public async Task Submit_QueuesSelectedMistakePositionAndReportsSynchronizationState()
    {
        var repository = new FakeMistakeRepository(MistakeSyncState.Synced);
        var viewModel = new MistakeReportViewModel(new QueueMistakeUseCase(repository));
        var sessionId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        viewModel.Initialize(sessionId, taskId, "مهمة اختبار", canRecordMistakes: true);
        viewModel.AyahId = "25";
        viewModel.PageNumber = "3";
        viewModel.WordIndex = "4";
        viewModel.SelectedMistakeType = MistakeType.Pronunciation;
        viewModel.Note = "موضع تجريبي";

        await viewModel.SubmitCommand.ExecuteAsync(null);

        var draft = Assert.IsType<MistakeDraft>(repository.LastDraft);
        Assert.Equal(sessionId, draft.SessionId);
        Assert.Equal(taskId, draft.TaskId);
        Assert.Equal(25, draft.AyahId);
        Assert.Equal(3, draft.PageNumber);
        Assert.Equal(4, draft.WordIndex);
        Assert.Equal(MistakeType.Pronunciation, draft.MistakeType);
        Assert.Equal("موضع تجريبي", draft.Note);
        Assert.NotEqual(Guid.Empty, draft.ClientOperationId);
        Assert.False(viewModel.IsError);
        Assert.Equal("تم إرسال الخطأ إلى الخادم بنجاح.", viewModel.Message);
        Assert.Empty(viewModel.AyahId);
        Assert.Empty(viewModel.WordIndex);
    }

    [Fact]
    public async Task Submit_WhenNetworkSyncIsPendingExplainsThatTheOperationWasQueued()
    {
        var viewModel = new MistakeReportViewModel(new QueueMistakeUseCase(new FakeMistakeRepository(MistakeSyncState.Pending)));
        viewModel.Initialize(Guid.NewGuid(), Guid.NewGuid(), "مهمة اختبار", canRecordMistakes: true);
        viewModel.AyahId = "1";
        viewModel.WordIndex = "1";

        await viewModel.SubmitCommand.ExecuteAsync(null);

        Assert.False(viewModel.IsError);
        Assert.Equal("حُفظ الخطأ محلياً وسيُعاد إرسالُه عند توفر الاتصال.", viewModel.Message);
    }

    [Theory]
    [InlineData(MistakeSyncState.Conflict, "تعارضت مزامنة الخطأ؛ راجع العملية قبل إعادة المحاولة.")]
    [InlineData(MistakeSyncState.Failed, "تعذر قبول الخطأ من الخادم.")]
    public async Task Submit_WhenSynchronizationCannotCompleteExplainsTheActualState(MistakeSyncState syncState, string expectedMessage)
    {
        var viewModel = new MistakeReportViewModel(new QueueMistakeUseCase(new FakeMistakeRepository(syncState)));
        viewModel.Initialize(Guid.NewGuid(), Guid.NewGuid(), "مهمة اختبار", canRecordMistakes: true);
        viewModel.AyahId = "1";
        viewModel.WordIndex = "1";

        await viewModel.SubmitCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsError);
        Assert.Equal(expectedMessage, viewModel.Message);
    }

    [Fact]
    public async Task Submit_WithInvalidPositionDoesNotQueueOperation()
    {
        var repository = new FakeMistakeRepository(MistakeSyncState.Pending);
        var viewModel = new MistakeReportViewModel(new QueueMistakeUseCase(repository));
        viewModel.Initialize(Guid.NewGuid(), Guid.NewGuid(), "مهمة اختبار", canRecordMistakes: true);
        viewModel.AyahId = "0";
        viewModel.WordIndex = "1";

        await viewModel.SubmitCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsError);
        Assert.Equal("رقم الآية يجب أن يكون بين 1 و6236.", viewModel.Message);
        Assert.Null(repository.LastDraft);
    }

    [Fact]
    public void Submit_IsAvailableWhenScreenIsInitializedForStudentParticipant()
    {
        var viewModel = new MistakeReportViewModel(new QueueMistakeUseCase(new FakeMistakeRepository(MistakeSyncState.Pending)));
        viewModel.Initialize(Guid.NewGuid(), Guid.NewGuid(), "مهمة اختبار", canRecordMistakes: true);

        Assert.True(viewModel.SubmitCommand.CanExecute(null));
    }

    private sealed class FakeMistakeRepository : IMistakeRepository
    {
        private readonly MistakeSyncState syncState;

        public FakeMistakeRepository(MistakeSyncState syncState)
        {
            this.syncState = syncState;
        }

        public MistakeDraft? LastDraft { get; private set; }

        public Task<Result<PendingMistakeOperation>> QueueCreateAsync(MistakeDraft draft, CancellationToken cancellationToken = default)
        {
            LastDraft = draft;
            var operation = new PendingMistakeOperation(Guid.NewGuid(), draft, syncState, DateTimeOffset.UtcNow, syncState == MistakeSyncState.Pending ? "الاتصال غير متاح" : null);
            return Task.FromResult(Result<PendingMistakeOperation>.Success(operation));
        }

        public Task<Result<IReadOnlyList<PendingMistakeOperation>>> SynchronizePendingAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<IReadOnlyList<PendingMistakeOperation>>.Success(Array.Empty<PendingMistakeOperation>()));
    }
}
