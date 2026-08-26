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
    public async Task LoadPage_LoadsLocalPageAndLabelsItsSource()
    {
        var repository = new FakeQuranRepository();
        var viewModel = new QuranReaderViewModel(new GetQuranPageUseCase(repository));
        viewModel.Initialize(2);

        await viewModel.LoadPageCommand.ExecuteAsync(null);

        Assert.Equal(2, repository.LastPageNumber);
        Assert.NotNull(viewModel.QuranPage);
        Assert.Equal(2, viewModel.QuranPage!.PageNumber);
        Assert.Contains("المحلية", viewModel.QuranSourceLabel);
        Assert.False(viewModel.IsError);
    }

    [Fact]
    public async Task NextPage_LoadsFollowingPageAndStopsAt604()
    {
        var repository = new FakeQuranRepository();
        var viewModel = new QuranReaderViewModel(new GetQuranPageUseCase(repository));
        viewModel.Initialize(603);
        await viewModel.LoadPageCommand.ExecuteAsync(null);

        await viewModel.LoadNextPageCommand.ExecuteAsync(null);

        Assert.Equal(604, repository.LastPageNumber);
        Assert.False(viewModel.LoadNextPageCommand.CanExecute(null));
    }

    [Fact]
    public async Task LoadPage_RejectsOutOfRangeInputWithoutRepositoryCall()
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
            LastPageNumber = pageNumber;
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
    }
}
