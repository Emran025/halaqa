using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Registrations.Domain.Entities;
using Halaqa.Desktop.Features.Registrations.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;
using Halaqa.Desktop.Shared.Presentation.Models;

namespace Halaqa.Desktop.Features.Registrations.Presentation.ViewModels;

public sealed partial class StudentRegistrationRequestsViewModel : ObservableObject
{
    private readonly ListMyRegistrationRequestsUseCase _listRequestsUseCase;
    private readonly CancelRegistrationRequestUseCase _cancelRequestUseCase;

    public StudentRegistrationRequestsViewModel(
        ListMyRegistrationRequestsUseCase listRequestsUseCase,
        CancelRegistrationRequestUseCase cancelRequestUseCase)
    {
        _listRequestsUseCase = listRequestsUseCase;
        _cancelRequestUseCase = cancelRequestUseCase;
    }

    public ObservableCollection<RegistrationRequest> Requests { get; } = new();
    public IReadOnlyList<LocalizedOption<string>> FilterOptions { get; } = new[]
    {
        new LocalizedOption<string>(string.Empty, "كل الطلبات"),
        new LocalizedOption<string>("pending", "قيد الانتظار"),
        new LocalizedOption<string>("completion_requested", "بانتظار استكمال البيانات"),
        new LocalizedOption<string>("accepted", "مقبول"),
        new LocalizedOption<string>("rejected", "مرفوض"),
        new LocalizedOption<string>("withdrawn", "مسحوب"),
        new LocalizedOption<string>("cancelled", "ملغى")
    };

    [ObservableProperty] private RegistrationRequest? _selectedRequest;
    [ObservableProperty] private string _filterState = string.Empty;
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _lastPage = 1;
    [ObservableProperty] private int _total;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private bool _hasActiveRequest;
    [ObservableProperty] private RegistrationRequest? _activeRequest;
    [ObservableProperty] private bool _canSearchNewTeacher;
    private bool _hasGlobalActiveOrAccepted;

    public bool HasNoRequests => !IsBusy && Requests.Count == 0;
    public bool HasRequests => !IsBusy && Requests.Count > 0;

    public string EditorTitle => SelectedRequest is null
        ? "اختر طلباً لعرض حالته"
        : $"طلب التسجيل بتاريخ {SelectedRequest.CreatedAt:yyyy-MM-dd}";

    public event EventHandler? BackRequested;
    public event EventHandler? SearchTeachersRequested;

    public void Initialize()
    {
        Requests.Clear();
        SelectedRequest = null;
        FilterState = string.Empty;
        CurrentPage = 1;
        LastPage = 1;
        Total = 0;
        HasActiveRequest = false;
        ActiveRequest = null;
        _hasGlobalActiveOrAccepted = false;
        CanSearchNewTeacher = false;
        ClearFeedback();
        NotifyCommands();
        NotifyStateChanges();
    }

    [RelayCommand]
    private void NewSearch() => SearchTeachersRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task SelectFilterTabAsync(string state)
    {
        FilterState = state ?? string.Empty;
        await LoadPageAsync(1);
    }

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadAsync() => await LoadPageAsync(1);

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task ApplyFilterAsync() => await LoadPageAsync(1);

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadNextPageAsync()
    {
        if (CurrentPage < LastPage)
        {
            await LoadPageAsync(CurrentPage + 1);
        }
    }

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadPreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            await LoadPageAsync(CurrentPage - 1);
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private async Task CancelAsync(RegistrationRequest? request)
    {
        var selected = request ?? SelectedRequest;
        if (selected is null)
        {
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _cancelRequestUseCase.ExecuteAsync(selected.Id);
            if (!result.IsSuccess)
            {
                SetFailure(result.Error);
                return;
            }

            await LoadPageAsync(CurrentPage);
            Message = "تم سحب طلب التسجيل بنجاح. يمكنك الآن البحث عن معلم جديد والتقديم.";
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    partial void OnSelectedRequestChanged(RegistrationRequest? value)
    {
        OnPropertyChanged(nameof(EditorTitle));
        CancelCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsBusyChanged(bool value)
    {
        NotifyStateChanges();
    }

    private bool CanLoad() => !IsBusy;
    private bool CanCancel(RegistrationRequest? request) =>
        !IsBusy && (request ?? SelectedRequest)?.State == RegistrationState.Pending;
    private bool CanNavigateBack() => !IsBusy;

    private async Task LoadPageAsync(int page)
    {
        if (!TryParseState(FilterState, out var state))
        {
            SetLocalFailure("اختر حالة طلب صحيحة.");
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _listRequestsUseCase.ExecuteAsync(state, page);
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            Requests.Clear();
            foreach (var request in result.Value.Requests)
            {
                Requests.Add(request);
            }
            CurrentPage = result.Value.CurrentPage;
            LastPage = result.Value.LastPage;
            Total = result.Value.Total;
            SelectedRequest = null;

            if (string.IsNullOrEmpty(FilterState))
            {
                _hasGlobalActiveOrAccepted = Requests.Any(r =>
                    r.State == RegistrationState.Pending ||
                    r.State == RegistrationState.CompletionRequested ||
                    r.State == RegistrationState.Accepted);
                CanSearchNewTeacher = !_hasGlobalActiveOrAccepted;

                ActiveRequest = Requests.FirstOrDefault(r =>
                    r.State == RegistrationState.Pending || r.State == RegistrationState.CompletionRequested);
                HasActiveRequest = ActiveRequest is not null;
            }
            else if (FilterState == "pending")
            {
                ActiveRequest = Requests.FirstOrDefault(r =>
                    r.State == RegistrationState.Pending || r.State == RegistrationState.CompletionRequested);
                HasActiveRequest = ActiveRequest is not null;
                if (HasActiveRequest)
                {
                    _hasGlobalActiveOrAccepted = true;
                    CanSearchNewTeacher = false;
                }
            }
            else if (FilterState == "accepted")
            {
                if (Requests.Any(r => r.State == RegistrationState.Accepted))
                {
                    _hasGlobalActiveOrAccepted = true;
                    CanSearchNewTeacher = false;
                }
            }
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
            NotifyStateChanges();
        }
    }

    private void NotifyCommands()
    {
        LoadCommand.NotifyCanExecuteChanged();
        ApplyFilterCommand.NotifyCanExecuteChanged();
        LoadNextPageCommand.NotifyCanExecuteChanged();
        LoadPreviousPageCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
        BackCommand.NotifyCanExecuteChanged();
    }

    private void NotifyStateChanges()
    {
        OnPropertyChanged(nameof(HasNoRequests));
        OnPropertyChanged(nameof(HasRequests));
    }

    private void ClearFeedback()
    {
        IsError = false;
        Message = null;
    }

    private void SetLocalFailure(string message)
    {
        ClearFeedback();
        IsError = true;
        Message = message;
    }

    private void SetFailure(AppError? error)
    {
        IsError = true;
        Message = error?.Message ?? "تعذر إتمام العملية. أعد المحاولة.";
    }

    private static bool TryParseState(string? value, out RegistrationState? state)
    {
        state = value switch
        {
            "" or null => null,
            "pending" => RegistrationState.Pending,
            "completion_requested" => RegistrationState.CompletionRequested,
            "accepted" => RegistrationState.Accepted,
            "rejected" => RegistrationState.Rejected,
            "withdrawn" => RegistrationState.Withdrawn,
            "cancelled" => RegistrationState.Cancelled,
            _ => null
        };
        return string.IsNullOrEmpty(value) || state is not null;
    }
}
