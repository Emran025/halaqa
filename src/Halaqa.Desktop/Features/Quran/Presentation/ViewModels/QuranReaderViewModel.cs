using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Quran.Domain.Entities;
using Halaqa.Desktop.Features.Quran.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Quran.Presentation.ViewModels;

public sealed class QuranReaderViewModel : ObservableObject
{
    private const int EditionId = 1;
    private const int FirstPage = 1;
    private const int LastPage = 604;
    private readonly GetQuranPageUseCase _getQuranPageUseCase;

    private QuranPage? _quranPage;
    private QuranAyah? _selectedAyah;
    private string _pageNumberInput = FirstPage.ToString();
    private bool _isLoading;
    private bool _isError;
    private string? _message;

    public QuranReaderViewModel(GetQuranPageUseCase getQuranPageUseCase)
    {
        _getQuranPageUseCase = getQuranPageUseCase;
        LoadPageCommand = new AsyncRelayCommand(LoadPageAsync, CanLoad);
        LoadPreviousPageCommand = new AsyncRelayCommand(LoadPreviousPageAsync, CanLoadPrevious);
        LoadNextPageCommand = new AsyncRelayCommand(LoadNextPageAsync, CanLoadNext);
        BackCommand = new RelayCommand(Back);
    }

    public AsyncRelayCommand LoadPageCommand { get; }
    public AsyncRelayCommand LoadPreviousPageCommand { get; }
    public AsyncRelayCommand LoadNextPageCommand { get; }
    public RelayCommand BackCommand { get; }

    public QuranPage? QuranPage
    {
        get => _quranPage;
        set
        {
            if (SetProperty(ref _quranPage, value))
            {
                LoadPreviousPageCommand.NotifyCanExecuteChanged();
                LoadNextPageCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(QuranSourceLabel));
            }
        }
    }

    public QuranAyah? SelectedAyah
    {
        get => _selectedAyah;
        set => SetProperty(ref _selectedAyah, value);
    }

    public string PageNumberInput
    {
        get => _pageNumberInput;
        set => SetProperty(ref _pageNumberInput, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
            {
                LoadPageCommand.NotifyCanExecuteChanged();
                LoadPreviousPageCommand.NotifyCanExecuteChanged();
                LoadNextPageCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool IsError
    {
        get => _isError;
        set => SetProperty(ref _isError, value);
    }

    public string? Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public string QuranSourceLabel => QuranPage is null
        ? "لم تُحمّل صفحة بعد."
        : QuranPage.IsFromLocalCache
            ? "الصفحة معروضة من قاعدة المصحف المحلية بخط QCF الخاص بها."
            : "الصفحة معروضة من المصدر البعيد كنص توافق؛ لا يضمن العقد رموز QCF الصفحية.";

    public event EventHandler? BackRequested;

    public void Initialize(int pageNumber = FirstPage)
    {
        QuranPage = null;
        SelectedAyah = null;
        PageNumberInput = Math.Clamp(pageNumber, FirstPage, LastPage).ToString();
        IsError = false;
        Message = null;
    }

    private async Task LoadPageAsync()
    {
        if (!TryReadPageNumber(out var pageNumber))
        {
            SetLocalFailure("أدخل رقم صفحة من 1 إلى 604.");
            return;
        }

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
            SelectedAyah = result.Value.Ayahs.FirstOrDefault();
            PageNumberInput = result.Value.PageNumber.ToString();
            Message = result.Value.IsFromLocalCache
                ? "تم تحميل الصفحة من قاعدة المصحف المحلية."
                : "تعذر المصدر المحلي، فعُرض بديل الخادم التوافقي.";
            OnPropertyChanged(nameof(QuranSourceLabel));
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadPreviousPageAsync()
    {
        PageNumberInput = (QuranPage?.PageNumber - 1 ?? FirstPage).ToString();
        await LoadPageAsync();
    }

    private async Task LoadNextPageAsync()
    {
        PageNumberInput = (QuranPage?.PageNumber + 1 ?? FirstPage).ToString();
        await LoadPageAsync();
    }

    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    private bool CanLoad() => !IsLoading;
    private bool CanLoadPrevious() => CanLoad() && (QuranPage?.PageNumber ?? FirstPage) > FirstPage;
    private bool CanLoadNext() => CanLoad() && (QuranPage?.PageNumber ?? FirstPage) < LastPage;

    private bool TryReadPageNumber(out int pageNumber) =>
        int.TryParse(PageNumberInput, out pageNumber) && pageNumber is >= FirstPage and <= LastPage;

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
}
