using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Presentation.ViewModels;

public sealed partial class SessionsViewModel : ObservableObject
{
    private const int PageSize = 20;
    private readonly ListSessionsUseCase listSessionsUseCase;

    public SessionsViewModel(ListSessionsUseCase listSessionsUseCase)
    {
        this.listSessionsUseCase = listSessionsUseCase;
    }

    public ObservableCollection<SessionListItem> Sessions { get; } = new();
    public IReadOnlyList<string> StateOptions { get; } = new[]
    {
        string.Empty,
        "requested",
        "accepted",
        "connecting",
        "directNegotiation",
        "connected",
        "weakConnection",
        "reconnecting",
        "disconnected",
        "directConnectionUnavailable",
        "ended",
        "cancelled",
        "rejected"
    };

    [ObservableProperty] private string _stateFilter = string.Empty;
    [ObservableProperty] private string? _from;
    [ObservableProperty] private string? _to;
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _lastPage = 1;
    [ObservableProperty] private int _total;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private string? _message;

    public event EventHandler? BackRequested;

    public void Initialize()
    {
        Sessions.Clear();
        StateFilter = string.Empty;
        From = null;
        To = null;
        CurrentPage = 1;
        LastPage = 1;
        Total = 0;
        ClearFeedback();
        NotifyCommands();
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
        if (!TryReadQuery(page, out var query, out var error))
        {
            SetLocalFailure(error!);
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await listSessionsUseCase.ExecuteAsync(query);
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            Sessions.Clear();
            foreach (var session in result.Value.Sessions)
            {
                Sessions.Add(session);
            }
            CurrentPage = result.Value.CurrentPage;
            LastPage = result.Value.LastPage;
            Total = result.Value.Total;
            if (Sessions.Count == 0)
            {
                Message = "لا توجد جلسات مطابقة للمرشحات الحالية.";
            }
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    private bool TryReadQuery(int page, out SessionQuery query, out string? error)
    {
        query = default!;
        error = null;
        if (!TryReadDateTime(From, out var from) || !TryReadDateTime(To, out var to))
        {
            error = "أدخل التاريخ بصيغة YYYY-MM-DD HH:mm +03:00 أو اتركه فارغاً.";
            return false;
        }
        if (from is { } fromValue && to is { } toValue && toValue < fromValue)
        {
            error = "لا يمكن أن يسبق تاريخ النهاية تاريخ البداية.";
            return false;
        }
        if (!TryReadState(out var state))
        {
            error = "حالة الجلسة المحددة غير صالحة.";
            return false;
        }

        query = new SessionQuery(null, null, state, from, to, page, PageSize);
        return true;
    }

    private bool TryReadState(out OfficialSessionState? state)
    {
        state = null;
        if (string.IsNullOrWhiteSpace(StateFilter))
        {
            return true;
        }
        if (!Enum.TryParse<OfficialSessionState>(StateFilter, ignoreCase: true, out var parsed))
        {
            return false;
        }

        state = parsed;
        return true;
    }

    private static bool TryReadDateTime(string? value, out DateTimeOffset? result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = null;
            return true;
        }

        var parsed = DateTimeOffset.TryParseExact(value, "yyyy-MM-dd HH:mm zzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime);
        result = parsed ? dateTime : null;
        return parsed;
    }

    private bool CanLoad() => !IsBusy;
    private bool CanLoadPrevious() => CanLoad() && CurrentPage > 1;
    private bool CanLoadNext() => CanLoad() && CurrentPage < LastPage;

    private void ClearFeedback()
    {
        IsError = false;
        Message = null;
    }

    private void SetLocalFailure(string message)
    {
        IsError = true;
        Message = message;
    }

    private void SetFailure(AppError? error)
    {
        IsError = true;
        Message = error?.Message ?? "تعذر تحميل قائمة الجلسات.";
    }

    private void NotifyCommands()
    {
        LoadCommand.NotifyCanExecuteChanged();
        ApplyFilterCommand.NotifyCanExecuteChanged();
        LoadPreviousPageCommand.NotifyCanExecuteChanged();
        LoadNextPageCommand.NotifyCanExecuteChanged();
    }
}
