using Halaqa.Desktop.Features.Quran.Domain.Entities;
using Halaqa.Desktop.Features.Quran.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Quran.Domain.UseCases;

public sealed class GetQuranIndexUseCase
{
    private readonly IQuranRepository _repository;

    public GetQuranIndexUseCase(IQuranRepository repository)
    {
        _repository = repository;
    }

    public Task<Result<IReadOnlyList<QuranSurahIndexItem>>> GetSurahsAsync(CancellationToken cancellationToken = default) =>
        _repository.GetSurahsIndexAsync(cancellationToken);

    public Task<Result<IReadOnlyList<QuranJuzIndexItem>>> GetJuzsAsync(CancellationToken cancellationToken = default) =>
        _repository.GetJuzIndexAsync(cancellationToken);
}
