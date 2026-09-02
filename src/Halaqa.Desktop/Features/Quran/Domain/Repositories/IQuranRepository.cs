using Halaqa.Desktop.Features.Quran.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Quran.Domain.Repositories;

public interface IQuranRepository
{
    Task<Result<QuranPage>> GetPageAsync(
        int editionId,
        int pageNumber,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<QuranSurahIndexItem>>> GetSurahsIndexAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<QuranJuzIndexItem>>> GetJuzIndexAsync(CancellationToken cancellationToken = default);
}
