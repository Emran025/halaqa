using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;
using Halaqa.Desktop.Shared.Presentation.Models;

namespace Halaqa.Desktop.Features.Sessions.Presentation.ViewModels;

/// <summary>
/// ViewModel لواجهة الطالب الخاصة بعرض الجلسات التاريخية (السابقة أو الجارية).
/// مبسّطة ومنظّمة من اليمين لليسار دون أزرار قبول/رفض.
/// </summary>
public sealed partial class StudentSessionsViewModel : ObservableObject
{
    private const int PageSize = 15;
    private readonly ListSessionsUseCase _listSessionsUseCase;

    public StudentSessionsViewModel(ListSessionsUseCase listSessionsUseCase)
    {
        _listSessionsUseCase = listSessionsUseCase;
    }

    public ObservableCollection<SessionListItem> Sessions { get; } = new();

    // الحالات المعروضة للطالب: لا تشمل "Requested" لأن تلك تُعالَج عبر الأوفرلاي
    public IReadOnlyList<LocalizedOption<string>> StateOptions { get; } = new[]
    {
        new LocalizedOption<string>(string.Empty,           "كل الجلسات"),
        new LocalizedOption<string>("accepted",             "مقبولة"),
        new LocalizedOption<string>("connected",            "متصلة"),
        new LocalizedOption<string>("disconnected",         "منقطعة"),
        new LocalizedOption<string>("ended",                "منتهية"),
        new LocalizedOption<string>("cancelled",            "ملغاة"),
        new LocalizedOption<string>("rejected",             "مرفوضة"),
    };

    [ObservableProperty] private string _stateFilter = string.Empty;
    [ObservableProperty] private int    _currentPage = 1;
    [ObservableProperty] private int    _lastPage    = 1;
    [ObservableProperty] private int    _total;
    [ObservableProperty] private bool   _isBusy;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private bool   _hasNoSessions;

    public event EventHandler? BackRequested;

    public void Initialize()
    {
        Sessions.Clear();
        StateFilter   = string.Empty;
        CurrentPage   = 1;
        LastPage      = 1;
        Total         = 0;
        Message       = null;
        HasNoSessions = false;
    }

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadAsync() => await LoadPageAsync(1);

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task ApplyFilterAsync() => await LoadPageAsync(1);

    [RelayCommand(CanExecute = nameof(CanLoadPrevious))]
    private async Task LoadPreviousPageAsync() => await LoadPageAsync(CurrentPage - 1);

    [RelayCommand(CanExecute = nameof(CanLoadNext))]
    private async Task LoadNextPageAsync() => await LoadPageAsync(CurrentPage + 1);

    [RelayCommand]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    private async Task LoadPageAsync(int page)
    {
        IsBusy = true;
        Message = null;
        try
        {
            OfficialSessionState? state = null;
            if (!string.IsNullOrWhiteSpace(StateFilter) &&
                Enum.TryParse<OfficialSessionState>(StateFilter, ignoreCase: true, out var parsed))
            {
                state = parsed;
            }

            var query = new SessionQuery(
                HalaqaId:  null,
                StudentId: null,   // السيرفر يعرف هوية المستخدم من التوكن
                State:     state,
                From:      null,
                To:        null,
                Page:      page,
                PerPage:   PageSize);

            var result = await _listSessionsUseCase.ExecuteAsync(query);
            if (!result.IsSuccess || result.Value is null)
            {
                Message = result.Error?.Message ?? "تعذر تحميل قائمة الجلسات.";
                return;
            }

            Sessions.Clear();
            foreach (var session in result.Value.Sessions)
                Sessions.Add(session);

            CurrentPage   = result.Value.CurrentPage;
            LastPage      = result.Value.LastPage;
            Total         = result.Value.Total;
            HasNoSessions = Sessions.Count == 0;

            if (HasNoSessions)
                Message = null; // الـ UI يعرض البطاقة الخاصة بـ "لا توجد جلسات"
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    private bool CanLoad()         => !IsBusy;
    private bool CanLoadPrevious() => !IsBusy && CurrentPage > 1;
    private bool CanLoadNext()     => !IsBusy && CurrentPage < LastPage;

    private void NotifyCommands()
    {
        LoadCommand.NotifyCanExecuteChanged();
        ApplyFilterCommand.NotifyCanExecuteChanged();
        LoadPreviousPageCommand.NotifyCanExecuteChanged();
        LoadNextPageCommand.NotifyCanExecuteChanged();
    }
}
