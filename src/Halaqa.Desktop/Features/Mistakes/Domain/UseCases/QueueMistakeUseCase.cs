using Halaqa.Desktop.Features.Mistakes.Domain.Entities;
using Halaqa.Desktop.Features.Mistakes.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Mistakes.Domain.UseCases;

public sealed class QueueMistakeUseCase
{

    private readonly IMistakeRepository repository;


    public QueueMistakeUseCase(

        IMistakeRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<PendingMistakeOperation>> ExecuteAsync(
        Guid sessionId,
        Guid taskId,
        int ayahId,
        int? pageNumber,
        int wordIndex,
        MistakeType mistakeType,
        string? note,
        CancellationToken cancellationToken = default)
    {
        if (sessionId == Guid.Empty || taskId == Guid.Empty || ayahId is < 1 or > 6236 ||
            pageNumber is < 1 or > 604 || wordIndex < 1 || note?.Length > 1000)
        {
            return Task.FromResult(Result<PendingMistakeOperation>.Failure(new AppError(
                AppErrorKind.Validation,
                "تعذر تسجيل الخطأ لأن موضعه أو تفاصيله غير صالحة.")));
        }

        var draft = new MistakeDraft(
            sessionId,
            taskId,
            ayahId,
            pageNumber,
            wordIndex,
            mistakeType,
            note,
            Guid.NewGuid());
        return repository.QueueCreateAsync(draft, cancellationToken);
    }
}
