using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Auth.Domain.Entities;
using Halaqa.Desktop.Features.Progress.Domain.UseCases;
using Halaqa.Desktop.Features.Quran.Domain.Entities;
using Halaqa.Desktop.Features.Quran.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Quran.Presentation.ViewModels;

public enum StudentPlanMode
{
    Memorization, // مصحف الحفظ
    Review,       // مصحف المراجعة
    Recitation    // مصحف السرد والتلاوة
}

public sealed partial class QuranReaderViewModel : ObservableObject
{
    private const int EditionId = 1;
    private const int FirstPage = 1;
    private const int LastPage = 604;

    private readonly GetQuranPageUseCase _getQuranPageUseCase;
    private readonly GetQuranIndexUseCase _getQuranIndexUseCase;
    private readonly GetStudentProgressUseCase? _getStudentProgressUseCase;

    private readonly List<QuranSurahIndexItem> _allSurahsMaster = new();

    [ObservableProperty] private Guid _studentId;
    [ObservableProperty] private UserRole? _userRole;
    [ObservableProperty] private QuranPage? _quranPage;
    [ObservableProperty] private QuranPage? _facingPage;
    [ObservableProperty] private QuranAyah? _selectedAyah;
    [ObservableProperty] private string _pageNumberInput = FirstPage.ToString();
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private string? _message;

    [ObservableProperty] private StudentPlanMode _currentPlanMode = StudentPlanMode.Memorization;
    [ObservableProperty] private int _memorizationLastPage = 1;
    [ObservableProperty] private int _reviewLastPage = 1;
    [ObservableProperty] private int _recitationLastPage = 1;

    [ObservableProperty] private bool _isIndexDialogOpen;
    [ObservableProperty] private string _selectedIndexTab = "Surahs";
    [ObservableProperty] private string _indexSearchText = string.Empty;

    public ObservableCollection<QuranSurahIndexItem> FilteredSurahs { get; } = new();
    public ObservableCollection<QuranJuzIndexItem> AllJuzs { get; } = new();
    public IReadOnlyList<int> AllPages { get; } = Enumerable.Range(1, 604).ToList();

    public bool IsMemorizationSelected => CurrentPlanMode == StudentPlanMode.Memorization;
    public bool IsReviewSelected => CurrentPlanMode == StudentPlanMode.Review;
    public bool IsRecitationSelected => CurrentPlanMode == StudentPlanMode.Recitation;

    public string RightSurahName => QuranPage?.Surahs.FirstOrDefault()?.Name
        ?? (QuranPage?.Ayahs.FirstOrDefault() is { } a ? $"سورة {a.SurahId}" : string.Empty);

    public string RightJuzName => QuranPage?.Ayahs.FirstOrDefault()?.Juz is { } j
        ? GetJuzName(j) : string.Empty;

    public int RightPageNumber => QuranPage?.PageNumber ?? 1;

    public string LeftSurahName => FacingPage?.Surahs.FirstOrDefault()?.Name
        ?? (FacingPage?.Ayahs.FirstOrDefault() is { } a ? $"سورة {a.SurahId}" : string.Empty);

    public string LeftJuzName => FacingPage?.Ayahs.FirstOrDefault()?.Juz is { } j
        ? GetJuzName(j) : string.Empty;

    public int LeftPageNumber => FacingPage?.PageNumber ?? 2;

    public bool HasFacingPage => FacingPage != null;

    public string CurrentSpreadTitle => FacingPage != null
        ? $"صفحة {RightPageNumber} - {LeftPageNumber}"
        : $"صفحة {RightPageNumber}";

    public string QuranSourceLabel => QuranPage is null
        ? "لم تُحمّل صفحة بعد."
        : QuranPage.IsFromLocalCache
            ? "الصفحة معروضة من قاعدة المصحف المحلية بالرسم العثماني."
            : "الصفحة معروضة من المصدر البعيد.";

    public event EventHandler? BackRequested;

    public QuranReaderViewModel(
        GetQuranPageUseCase getQuranPageUseCase,
        GetQuranIndexUseCase getQuranIndexUseCase,
        GetStudentProgressUseCase? getStudentProgressUseCase = null)
    {
        _getQuranPageUseCase = getQuranPageUseCase;
        _getQuranIndexUseCase = getQuranIndexUseCase;
        _getStudentProgressUseCase = getStudentProgressUseCase;
    }

    public void Initialize(Guid studentId = default, UserRole? role = null, int pageNumber = FirstPage)
    {
        StudentId = studentId;
        UserRole = role;
        QuranPage = null;
        FacingPage = null;
        SelectedAyah = null;
        CurrentPlanMode = StudentPlanMode.Memorization;
        PageNumberInput = NormalizeSpreadStart(Math.Clamp(pageNumber, FirstPage, LastPage)).ToString();
        IsError = false;
        Message = null;
        IsIndexDialogOpen = false;
        NotifyPlanSelections();

        if (studentId != Guid.Empty && _getStudentProgressUseCase != null)
        {
            _ = LoadStudentProgressAsync(studentId);
        }
    }

    private async Task LoadStudentProgressAsync(Guid studentId)
    {
        try
        {
            var result = await _getStudentProgressUseCase!.ExecuteAsync(studentId, null);
            if (result.IsSuccess && result.Value is { } progress)
            {
                if (progress.LastCompleted.Memorization?.StartPage is { } mPage && mPage >= 1 && mPage <= 604)
                    MemorizationLastPage = mPage;
                if (progress.LastCompleted.Review?.StartPage is { } rPage && rPage >= 1 && rPage <= 604)
                    ReviewLastPage = rPage;
                if (progress.LastCompleted.Recitation?.StartPage is { } sPage && sPage >= 1 && sPage <= 604)
                    RecitationLastPage = sPage;

                var target = CurrentPlanMode switch
                {
                    StudentPlanMode.Review => ReviewLastPage,
                    StudentPlanMode.Recitation => RecitationLastPage,
                    _ => MemorizationLastPage
                };

                await LoadPageByNumberAsync(target);
            }
        }
        catch
        {
            // Ignore progress fetch failure
        }
    }

    [RelayCommand]
    private async Task SwitchToMemorizationPlanAsync()
    {
        SaveCurrentPageToCurrentMode();
        CurrentPlanMode = StudentPlanMode.Memorization;
        NotifyPlanSelections();
        await LoadPageByNumberAsync(MemorizationLastPage);
    }

    [RelayCommand]
    private async Task SwitchToReviewPlanAsync()
    {
        SaveCurrentPageToCurrentMode();
        CurrentPlanMode = StudentPlanMode.Review;
        NotifyPlanSelections();
        await LoadPageByNumberAsync(ReviewLastPage);
    }

    [RelayCommand]
    private async Task SwitchToRecitationPlanAsync()
    {
        SaveCurrentPageToCurrentMode();
        CurrentPlanMode = StudentPlanMode.Recitation;
        NotifyPlanSelections();
        await LoadPageByNumberAsync(RecitationLastPage);
    }

    private void SaveCurrentPageToCurrentMode()
    {
        var currentPage = QuranPage?.PageNumber ?? 1;
        switch (CurrentPlanMode)
        {
            case StudentPlanMode.Memorization:
                MemorizationLastPage = currentPage;
                break;
            case StudentPlanMode.Review:
                ReviewLastPage = currentPage;
                break;
            case StudentPlanMode.Recitation:
                RecitationLastPage = currentPage;
                break;
        }
    }

    private void NotifyPlanSelections()
    {
        OnPropertyChanged(nameof(IsMemorizationSelected));
        OnPropertyChanged(nameof(IsReviewSelected));
        OnPropertyChanged(nameof(IsRecitationSelected));
    }

    [RelayCommand(CanExecute = nameof(CanLoadPrevious))]
    public async Task LoadPreviousSpreadAsync()
    {
        var prevPage = Math.Max(FirstPage, (QuranPage?.PageNumber ?? FirstPage) - 2);
        await LoadPageByNumberAsync(prevPage);
    }

    [RelayCommand(CanExecute = nameof(CanLoadNext))]
    public async Task LoadNextSpreadAsync()
    {
        var nextPage = Math.Min(LastPage - 1, (QuranPage?.PageNumber ?? FirstPage) + 2);
        await LoadPageByNumberAsync(nextPage);
    }

    [RelayCommand(CanExecute = nameof(CanLoad))]
    public async Task LoadPageCommandExecuteAsync()
    {
        if (TryReadPageNumber(out var pageNumber))
        {
            await LoadPageByNumberAsync(pageNumber);
        }
        else
        {
            SetLocalFailure("أدخل رقم صفحة من 1 إلى 604.");
        }
    }

    // Command aliases to maintain full backwards compatibility with any existing bindings or tests
    public IAsyncRelayCommand LoadPageCommand => new AsyncRelayCommand(LoadPageCommandExecuteAsync, CanLoad);
    public IAsyncRelayCommand LoadNextPageCommand => new AsyncRelayCommand(LoadNextSpreadAsync, CanLoadNext);
    public IAsyncRelayCommand LoadPreviousPageCommand => new AsyncRelayCommand(LoadPreviousSpreadAsync, CanLoadPrevious);
    public IRelayCommand BackCommand => new RelayCommand(Back);

    public async Task LoadPageByNumberAsync(int pageNumber)
    {
        pageNumber = Math.Clamp(pageNumber, FirstPage, LastPage);
        pageNumber = NormalizeSpreadStart(pageNumber);

        IsLoading = true;
        ClearFeedback();
        try
        {
            var result = await _getQuranPageUseCase.ExecuteAsync(EditionId, pageNumber);
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            QuranPage = result.Value;
            FacingPage = null;

            if (pageNumber < LastPage)
            {
                var facingResult = await _getQuranPageUseCase.ExecuteAsync(EditionId, pageNumber + 1);
                if (facingResult.IsSuccess)
                {
                    FacingPage = facingResult.Value;
                }
            }

            SelectedAyah = result.Value.Ayahs.FirstOrDefault();
            PageNumberInput = result.Value.PageNumber.ToString();
            SaveCurrentPageToCurrentMode();
            NotifyPageInfoChanged();
        }
        finally
        {
            IsLoading = false;
            NotifyNavigationCommands();
        }
    }

    [RelayCommand]
    public async Task OpenIndexDialogAsync()
    {
        await EnsureIndexLoadedAsync();
        IsIndexDialogOpen = true;
    }

    [RelayCommand]
    public void CloseIndexDialog() => IsIndexDialogOpen = false;

    [RelayCommand]
    public void SelectIndexTab(string? tab)
    {
        if (!string.IsNullOrEmpty(tab))
            SelectedIndexTab = tab;
    }

    [RelayCommand]
    public async Task SelectSurahAsync(QuranSurahIndexItem? surah)
    {
        if (surah == null) return;
        IsIndexDialogOpen = false;
        await LoadPageByNumberAsync(surah.StartPage);
    }

    [RelayCommand]
    public async Task SelectJuzAsync(QuranJuzIndexItem? juz)
    {
        if (juz == null) return;
        IsIndexDialogOpen = false;
        await LoadPageByNumberAsync(juz.StartPage);
    }

    [RelayCommand]
    public async Task SelectPageAsync(int pageNumber)
    {
        IsIndexDialogOpen = false;
        await LoadPageByNumberAsync(pageNumber);
    }

    private async Task EnsureIndexLoadedAsync()
    {
        if (_allSurahsMaster.Count == 0)
        {
            var surahsResult = await _getQuranIndexUseCase.GetSurahsAsync();
            if (surahsResult.IsSuccess && surahsResult.Value != null)
            {
                _allSurahsMaster.AddRange(surahsResult.Value);
                ApplySurahFilter();
            }

            var juzResult = await _getQuranIndexUseCase.GetJuzsAsync();
            if (juzResult.IsSuccess && juzResult.Value != null)
            {
                AllJuzs.Clear();
                foreach (var j in juzResult.Value)
                    AllJuzs.Add(j);
            }
        }
    }

    partial void OnIndexSearchTextChanged(string value) => ApplySurahFilter();

    private void ApplySurahFilter()
    {
        FilteredSurahs.Clear();
        var query = string.IsNullOrWhiteSpace(IndexSearchText)
            ? _allSurahsMaster
            : _allSurahsMaster.Where(s => s.Name.Contains(IndexSearchText.Trim(), StringComparison.OrdinalIgnoreCase));
        foreach (var s in query)
            FilteredSurahs.Add(s);
    }

    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    private bool CanLoad() => !IsLoading;
    private bool CanLoadPrevious() => CanLoad() && (QuranPage?.PageNumber ?? FirstPage) > FirstPage;
    private bool CanLoadNext() => CanLoad() && (QuranPage?.PageNumber ?? FirstPage) < LastPage - 1;

    private static int NormalizeSpreadStart(int pageNumber) =>
        pageNumber == LastPage ? LastPage - 1 : pageNumber % 2 == 0 ? pageNumber - 1 : pageNumber;

    private bool TryReadPageNumber(out int pageNumber) =>
        int.TryParse(PageNumberInput, out pageNumber) && pageNumber is >= FirstPage and <= LastPage;

    private void NotifyPageInfoChanged()
    {
        OnPropertyChanged(nameof(RightSurahName));
        OnPropertyChanged(nameof(RightJuzName));
        OnPropertyChanged(nameof(RightPageNumber));
        OnPropertyChanged(nameof(LeftSurahName));
        OnPropertyChanged(nameof(LeftJuzName));
        OnPropertyChanged(nameof(LeftPageNumber));
        OnPropertyChanged(nameof(HasFacingPage));
        OnPropertyChanged(nameof(CurrentSpreadTitle));
        OnPropertyChanged(nameof(QuranSourceLabel));
    }

    private void NotifyNavigationCommands()
    {
        LoadPreviousSpreadCommand.NotifyCanExecuteChanged();
        LoadNextSpreadCommand.NotifyCanExecuteChanged();
        LoadPageCommandExecuteCommand.NotifyCanExecuteChanged();
    }

    private void ClearFeedback()
    {
        IsError = false;
        Message = null;
    }

    private void SetFailure(AppError? error)
    {
        IsError = true;
        Message = error?.Message ?? "تعذر تحميل صفحة المصحف.";
        OnPropertyChanged(nameof(QuranSourceLabel));
    }

    private void SetLocalFailure(string message)
    {
        IsError = true;
        Message = message;
    }

    private static string GetJuzName(int number) => number switch
    {
        1 => "الجزء الأول",
        2 => "الجزء الثاني",
        3 => "الجزء الثالث",
        4 => "الجزء الرابع",
        5 => "الجزء الخامس",
        6 => "الجزء السادس",
        7 => "الجزء السابع",
        8 => "الجزء الثامن",
        9 => "الجزء التاسع",
        10 => "الجزء العاشر",
        11 => "الجزء الحادي عشر",
        12 => "الجزء الثاني عشر",
        13 => "الجزء الثالث عشر",
        14 => "الجزء الرابع عشر",
        15 => "الجزء الخامس عشر",
        16 => "الجزء السادس عشر",
        17 => "الجزء السابع عشر",
        18 => "الجزء الثامن عشر",
        19 => "الجزء التاسع عشر",
        20 => "الجزء العشرون",
        21 => "الجزء الحادي والعشرون",
        22 => "الجزء الثاني والعشرون",
        23 => "الجزء الثالث والعشرون",
        24 => "الجزء الرابع والعشرون",
        25 => "الجزء الخامس والعشرون",
        26 => "الجزء السادس والعشرون",
        27 => "الجزء السابع والعشرون",
        28 => "الجزء الثامن والعشرون",
        29 => "الجزء التاسع والعشرون",
        30 => "الجزء الثلاثون",
        _ => $"الجزء {number}"
    };
}
