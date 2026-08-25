using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Registrations.Domain.Entities;
using Halaqa.Desktop.Features.Registrations.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

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

    public ObservableCollection<RegistrationRequest> Requests { get; } = [];
    public IReadOnlyList<string> FilterOptions { get; } =
    [
        "",
        "pending",
        "completion_requested",
        "accepted",
        "rejected",
        "withdrawn",
        "cancelled"
    ];

    [ObservableProperty] private RegistrationRequest? _selectedRequest;
    [ObservableProperty] private string _filterState = string.Empty;
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _lastPage = 1;
    [ObservableProperty] private int _total;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private string? _message;

    public string EditorTitle => SelectedRequest is null
        ? "اختر طلباً لعرض حالته"
        : $"طلب التسجيل بتاريخ {SelectedRequest.CreatedAt:yyyy-MM-dd}";

    public event EventHandler? BackRequested;

    public void Initialize()
    {
        Requests.Clear();
        SelectedRequest = null;
        FilterState = string.Empty;
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
    private async Task CancelAsync()
    {
        var selected = SelectedRequest;
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
            Message = "تم سحب طلب التسجيل. يعرض الخادم الحالة الرسمية بعد إعادة التحميل.";
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

    private bool CanLoad() => !IsBusy;
    private bool CanCancel() => !IsBusy && SelectedRequest?.State == RegistrationState.Pending;
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
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
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
