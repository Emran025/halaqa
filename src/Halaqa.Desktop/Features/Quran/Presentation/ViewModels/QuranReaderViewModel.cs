using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Quran.Domain.Entities;
using Halaqa.Desktop.Features.Quran.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Quran.Presentation.ViewModels;

public sealed partial class QuranReaderViewModel : ObservableObject
{
    private const int EditionId = 1;
    private const int FirstPage = 1;
    private const int LastPage = 604;
    private readonly GetQuranPageUseCase _getQuranPageUseCase;

    public QuranReaderViewModel(GetQuranPageUseCase getQuranPageUseCase)
    {
        _getQuranPageUseCase = getQuranPageUseCase;
    }

    [ObservableProperty] private QuranPage? _quranPage;
    [ObservableProperty] private QuranAyah? _selectedAyah;
    [ObservableProperty] private string _pageNumberInput = FirstPage.ToString();
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private string? _message;

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

    [RelayCommand(CanExecute = nameof(CanLoad))]
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
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanLoadPrevious))]
    private async Task LoadPreviousPageAsync()
    {
        PageNumberInput = (QuranPage?.PageNumber - 1 ?? FirstPage).ToString();
        await LoadPageAsync();
    }

    [RelayCommand(CanExecute = nameof(CanLoadNext))]
    private async Task LoadNextPageAsync()
    {
        PageNumberInput = (QuranPage?.PageNumber + 1 ?? FirstPage).ToString();
        await LoadPageAsync();
    }

    [RelayCommand]
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

    private void NotifyCommands()
    {
        LoadPageCommand.NotifyCanExecuteChanged();
        LoadPreviousPageCommand.NotifyCanExecuteChanged();
        LoadNextPageCommand.NotifyCanExecuteChanged();
    }

    partial void OnQuranPageChanged(QuranPage? value) => NotifyCommands();
}
