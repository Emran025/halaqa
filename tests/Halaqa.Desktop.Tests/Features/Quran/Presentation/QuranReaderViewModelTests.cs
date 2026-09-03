using Halaqa.Desktop.Features.Quran.Domain.Entities;
using Halaqa.Desktop.Features.Quran.Domain.Repositories;
using Halaqa.Desktop.Features.Quran.Domain.UseCases;
using Halaqa.Desktop.Features.Quran.Presentation.ViewModels;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Quran.Presentation;

public sealed class QuranReaderViewModelTests
{
    [Fact]
    public async Task Initialize_LoadsFirstPageSuccessfully()
    {
        var repository = new FakeQuranRepository();
        var viewModel = new QuranReaderViewModel(new GetQuranPageUseCase(repository));

        viewModel.Initialize();
        await viewModel.LoadPageCommand.ExecuteAsync(null);

        Assert.Equal(1, repository.LastPageNumber);
        Assert.NotNull(viewModel.QuranPage);
        Assert.False(viewModel.IsError);
    }

    [Fact]
    public async Task LoadPageCommand_AdvancesToNextPage()
    {
        var repository = new FakeQuranRepository();
        var viewModel = new QuranReaderViewModel(new GetQuranPageUseCase(repository));
        viewModel.Initialize();

        await viewModel.LoadNextPageCommand.ExecuteAsync(null);

        Assert.Equal(3, repository.LastPageNumber);
    }

    [Fact]
    public async Task LoadPageCommand_WithInvalidInput_SetsErrorWithoutCallingRepository()
    {
        var repository = new FakeQuranRepository();
        var viewModel = new QuranReaderViewModel(new GetQuranPageUseCase(repository));
        viewModel.Initialize();
        viewModel.PageNumberInput = "605";

        await viewModel.LoadPageCommand.ExecuteAsync(null);

        Assert.Equal(0, repository.CallCount);
        Assert.True(viewModel.IsError);
        Assert.Contains("1 إلى 604", viewModel.Message);
    }

    private sealed class FakeQuranRepository : IQuranRepository
    {
        public int LastPageNumber { get; private set; }
        public int CallCount { get; private set; }

        public Task<Result<QuranPage>> GetPageAsync(int editionId, int pageNumber, CancellationToken cancellationToken = default)
        {
            if (CallCount == 0)
            {
                LastPageNumber = pageNumber;
            }
            CallCount++;
            var ayah = new QuranAyah(
                pageNumber,
                editionId,
                1,
                1,
                pageNumber,
                "بِسْمِ اللَّهِ الرَّحْمَٰنِ الرَّحِيمِ",
                "ﱁﱂﱃ",
                1,
                new[] { new QuranWord(0, "ﱁ") });
            return Task.FromResult(Result<QuranPage>.Success(new QuranPage(editionId, pageNumber, Array.Empty<QuranSurah>(), new[] { ayah }, IsFromLocalCache: true)));
        }

        public Task<Result<IReadOnlyList<QuranSurahIndexItem>>> GetSurahsIndexAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<IReadOnlyList<QuranSurahIndexItem>>.Success(new[]
            {
                new QuranSurahIndexItem(1, "الفاتحة", 7, 1, "مكية")
            }));

        public Task<Result<IReadOnlyList<QuranJuzIndexItem>>> GetJuzIndexAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<IReadOnlyList<QuranJuzIndexItem>>.Success(new[]
            {
                new QuranJuzIndexItem(1, "الجزء 1", 1, 21)
            }));
    }
}
