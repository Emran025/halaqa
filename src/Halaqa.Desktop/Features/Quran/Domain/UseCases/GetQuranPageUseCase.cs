using Halaqa.Desktop.Features.Quran.Domain.Entities;
using Halaqa.Desktop.Features.Quran.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Quran.Domain.UseCases;

public sealed class GetQuranPageUseCase(IQuranRepository repository)
{
    public Task<Result<QuranPage>> ExecuteAsync(
        int editionId,
        int pageNumber,
        CancellationToken cancellationToken = default)
    {
        if (editionId < 1 || pageNumber is < 1 or > 604)
        {
            return Task.FromResult(Result<QuranPage>.Failure(new AppError(
                AppErrorKind.Validation,
                "اختر إصداراً صالحاً ورقم صفحة بين 1 و604.")));
        }

        return repository.GetPageAsync(editionId, pageNumber, cancellationToken);
    }
}
