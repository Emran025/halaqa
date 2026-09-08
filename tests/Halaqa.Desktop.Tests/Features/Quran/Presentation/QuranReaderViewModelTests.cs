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
    public async Task Initialize_LoadsFirstPageAndFacingPageSuccessfully()
    {
        var repository = new FakeQuranRepository();
        var viewModel = new QuranReaderViewModel(new GetQuranPageUseCase(repository), new GetQuranIndexUseCase(repository));

        viewModel.Initialize();
        await viewModel.LoadPageCommand.ExecuteAsync(null);

        Assert.Equal(1, repository.LastPageNumber);
        Assert.NotNull(viewModel.QuranPage);
        Assert.Equal(1, viewModel.QuranPage!.PageNumber);
        Assert.NotNull(viewModel.FacingPage);
        Assert.Equal(2, viewModel.FacingPage!.PageNumber);
        Assert.Equal("الفاتحة", viewModel.RightSurahName);
        Assert.Equal("الجزء الأول", viewModel.RightJuzName);
        Assert.False(viewModel.IsError);
    }

    [Fact]
    public async Task LoadNextSpread_AdvancesByTwoPages()
    {
        var repository = new FakeQuranRepository();
        var viewModel = new QuranReaderViewModel(new GetQuranPageUseCase(repository), new GetQuranIndexUseCase(repository));
        viewModel.Initialize();
        await viewModel.LoadPageCommand.ExecuteAsync(null);

        await viewModel.LoadNextPageCommand.ExecuteAsync(null);

        Assert.Equal(3, viewModel.QuranPage?.PageNumber);
        Assert.Equal(4, viewModel.FacingPage?.PageNumber);
    }

    [Fact]
    public async Task LoadPreviousSpread_DecrementsByTwoPages()
    {
        var repository = new FakeQuranRepository();
        var viewModel = new QuranReaderViewModel(new GetQuranPageUseCase(repository), new GetQuranIndexUseCase(repository));
        viewModel.Initialize();
        await viewModel.LoadPageByNumberAsync(5);

        Assert.Equal(5, viewModel.QuranPage?.PageNumber);
        Assert.Equal(6, viewModel.FacingPage?.PageNumber);

        await viewModel.LoadPreviousSpreadCommand.ExecuteAsync(null);

        Assert.Equal(3, viewModel.QuranPage?.PageNumber);
        Assert.Equal(4, viewModel.FacingPage?.PageNumber);
    }

    [Fact]
    public async Task PlanSwitching_MaintainsDistinctPositionsForEachPlan()
    {
        var repository = new FakeQuranRepository();
        var viewModel = new QuranReaderViewModel(new GetQuranPageUseCase(repository), new GetQuranIndexUseCase(repository));
        viewModel.Initialize();

        // Start in Memorization mode at page 1
        await viewModel.LoadPageByNumberAsync(1);
        Assert.True(viewModel.IsMemorizationSelected);
        Assert.Equal(1, viewModel.MemorizationLastPage);

        // Advance to page 5 in Memorization mode
        await viewModel.LoadPageByNumberAsync(5);
        Assert.Equal(5, viewModel.MemorizationLastPage);

        // Switch to Review mode and set to page 101
        await viewModel.SwitchToReviewPlanCommand.ExecuteAsync(null);
        Assert.True(viewModel.IsReviewSelected);
        await viewModel.LoadPageByNumberAsync(101);
        Assert.Equal(101, viewModel.ReviewLastPage);

        // Switch to Recitation mode and set to page 201
        await viewModel.SwitchToRecitationPlanCommand.ExecuteAsync(null);
        Assert.True(viewModel.IsRecitationSelected);
        await viewModel.LoadPageByNumberAsync(201);
        Assert.Equal(201, viewModel.RecitationLastPage);

        // Switch back to Memorization mode: should automatically jump to page 5!
        await viewModel.SwitchToMemorizationPlanCommand.ExecuteAsync(null);
        Assert.True(viewModel.IsMemorizationSelected);
        Assert.Equal(5, viewModel.QuranPage?.PageNumber);

        // Switch back to Review mode: should automatically jump to page 101!
        await viewModel.SwitchToReviewPlanCommand.ExecuteAsync(null);
        Assert.True(viewModel.IsReviewSelected);
        Assert.Equal(101, viewModel.QuranPage?.PageNumber);
    }

    [Fact]
    public async Task NavigationIndex_SelectSurahAndJuz_JumpsToStartPage()
    {
        var repository = new FakeQuranRepository();
        var viewModel = new QuranReaderViewModel(new GetQuranPageUseCase(repository), new GetQuranIndexUseCase(repository));
        viewModel.Initialize();

        // Open index
        await viewModel.OpenIndexDialogCommand.ExecuteAsync(null);
        Assert.True(viewModel.IsIndexDialogOpen);
        Assert.NotEmpty(viewModel.FilteredSurahs);

        // Select Surah starting at page 50
        var surah = new QuranSurahIndexItem(2, "البقرة", 286, 50, "مدنية");
        await viewModel.SelectSurahCommand.ExecuteAsync(surah);

        Assert.False(viewModel.IsIndexDialogOpen);
        // Spread starts on odd page 49
        Assert.Equal(49, viewModel.QuranPage?.PageNumber);
        Assert.Equal(50, viewModel.FacingPage?.PageNumber);
    }

    [Fact]
    public async Task LoadPageCommand_WithInvalidInput_SetsErrorWithoutCallingRepository()
    {
        var repository = new FakeQuranRepository();
        var viewModel = new QuranReaderViewModel(new GetQuranPageUseCase(repository), new GetQuranIndexUseCase(repository));
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
            var surah = new QuranSurah(1, editionId, 1, "الفاتحة", 7, "مكية");
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
            return Task.FromResult(Result<QuranPage>.Success(new QuranPage(editionId, pageNumber, new[] { surah }, new[] { ayah }, IsFromLocalCache: true)));
        }

        public Task<Result<IReadOnlyList<QuranSurahIndexItem>>> GetSurahsIndexAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<IReadOnlyList<QuranSurahIndexItem>>.Success(new[]
            {
                new QuranSurahIndexItem(1, "الفاتحة", 7, 1, "مكية"),
                new QuranSurahIndexItem(2, "البقرة", 286, 2, "مدنية")
            }));

        public Task<Result<IReadOnlyList<QuranJuzIndexItem>>> GetJuzIndexAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<IReadOnlyList<QuranJuzIndexItem>>.Success(new[]
            {
                new QuranJuzIndexItem(1, "الجزء 1", 1, 21)
            }));
    }
}
