using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Registrations.Domain.Entities;
using Halaqa.Desktop.Features.Registrations.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Registrations.Presentation.ViewModels;

public sealed partial class TeacherApplicationInboxViewModel : ObservableObject
{
    private readonly ListTeacherApplicationInboxUseCase _listApplicationsUseCase;
    private readonly AcceptRegistrationRequestUseCase _acceptRequestUseCase;
    private readonly RejectRegistrationRequestUseCase _rejectRequestUseCase;
    private readonly RequestRegistrationCompletionUseCase _requestCompletionUseCase;

    public TeacherApplicationInboxViewModel(
        ListTeacherApplicationInboxUseCase listApplicationsUseCase,
        AcceptRegistrationRequestUseCase acceptRequestUseCase,
        RejectRegistrationRequestUseCase rejectRequestUseCase,
        RequestRegistrationCompletionUseCase requestCompletionUseCase)
    {
        _listApplicationsUseCase = listApplicationsUseCase;
        _acceptRequestUseCase = acceptRequestUseCase;
        _rejectRequestUseCase = rejectRequestUseCase;
        _requestCompletionUseCase = requestCompletionUseCase;
    }

    public ObservableCollection<RegistrationRequest> Requests { get; } = [];
    public IReadOnlyList<string> FilterOptions { get; } = ["", "pending", "completion_requested", "accepted", "rejected", "withdrawn", "cancelled"];

    [ObservableProperty] private RegistrationRequest? _selectedRequest;
    [ObservableProperty] private string _filterState = "pending";
    [ObservableProperty] private string? _searchText;
    [ObservableProperty] private string? _rejectionNote;
    [ObservableProperty] private string _requiredFields = string.Empty;
    [ObservableProperty] private string? _completionNote;
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _lastPage = 1;
    [ObservableProperty] private int _total;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private string? _message;

    public string EditorTitle => SelectedRequest is null ? "اختر متقدماً لمراجعته" : $"مراجعة طلب {SelectedRequest.Applicant.DisplayName}";
    public event EventHandler? BackRequested;

    public void Initialize()
    {
        Requests.Clear();
        SelectedRequest = null;
        FilterState = "pending";
        SearchText = null;
        RejectionNote = null;
        RequiredFields = string.Empty;
        CompletionNote = null;
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
    private async Task LoadPreviousPageAsync()
    {
        if (CurrentPage > 1) await LoadPageAsync(CurrentPage - 1);
    }

    [RelayCommand(CanExecute = nameof(CanLoadNext))]
    private async Task LoadNextPageAsync()
    {
        if (CurrentPage < LastPage) await LoadPageAsync(CurrentPage + 1);
    }

    [RelayCommand(CanExecute = nameof(CanDecide))]
    private async Task AcceptAsync()
    {
        if (SelectedRequest is not { } selected) return;
        await RunDecisionAsync(async () => await _acceptRequestUseCase.ExecuteAsync(selected.Id), "تم قبول طلب التسجيل. تظهر العلاقة التعليمية الناتجة وفق إجراءات الخادم.");
    }

    [RelayCommand(CanExecute = nameof(CanDecide))]
    private async Task RejectAsync()
    {
        if (SelectedRequest is not { } selected) return;
        await RunDecisionAsync(async () => await _rejectRequestUseCase.ExecuteAsync(new RejectRegistrationRequestCommand(selected.Id, NormalizeOptional(RejectionNote))), "تم رفض طلب التسجيل.");
        RejectionNote = null;
    }

    [RelayCommand(CanExecute = nameof(CanRequestCompletion))]
    private async Task RequestCompletionAsync()
    {
        if (SelectedRequest is not { } selected) return;
        var fields = ParseRequiredFields();
        if (fields.Count == 0)
        {
            SetLocalFailure("أضف حقلاً واحداً على الأقل لطلب الاستكمال.");
            return;
        }

        await RunDecisionAsync(async () => await _requestCompletionUseCase.ExecuteAsync(new RequestRegistrationCompletionCommand(selected.Id, fields, NormalizeOptional(CompletionNote))), "تم إرسال طلب استكمال البيانات.");
        RequiredFields = string.Empty;
        CompletionNote = null;
    }

    [RelayCommand]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    private async Task RunDecisionAsync(Func<Task<Result<RegistrationRequest>>> operation, string successMessage)
    {
        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await operation();
            if (!result.IsSuccess || result.Value is null) { SetFailure(result.Error); return; }
            Upsert(result.Value);
            SelectedRequest = result.Value;
            Message = successMessage;
        }
        finally { IsBusy = false; NotifyCommands(); }
    }

    private async Task LoadPageAsync(int page)
    {
        if (!TryParseState(FilterState, out var state)) { SetLocalFailure("اختر حالة طلب صحيحة."); return; }
        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _listApplicationsUseCase.ExecuteAsync(state, SearchText, page);
            if (!result.IsSuccess || result.Value is null) { SetFailure(result.Error); return; }
            Requests.Clear();
            foreach (var request in result.Value.Requests) Requests.Add(request);
            CurrentPage = result.Value.CurrentPage;
            LastPage = result.Value.LastPage;
            Total = result.Value.Total;
            SelectedRequest = Requests.FirstOrDefault();
        }
        finally { IsBusy = false; NotifyCommands(); }
    }

    private bool CanLoad() => !IsBusy;
    private bool CanLoadPrevious() => CanLoad() && CurrentPage > 1;
    private bool CanLoadNext() => CanLoad() && CurrentPage < LastPage;
    private bool CanDecide() => !IsBusy && SelectedRequest is not null;
    private bool CanRequestCompletion() => CanDecide() && ParseRequiredFields().Count > 0;

    partial void OnSelectedRequestChanged(RegistrationRequest? value)
    {
        RejectionNote = null; RequiredFields = string.Empty; CompletionNote = null;
        OnPropertyChanged(nameof(EditorTitle)); NotifyCommands();
    }
    partial void OnRequiredFieldsChanged(string value) => RequestCompletionCommand.NotifyCanExecuteChanged();

    private void Upsert(RegistrationRequest request)
    {
        var existing = Requests.Select((value, index) => (value, index)).FirstOrDefault(item => item.value.Id == request.Id);
        if (existing.value is null) Requests.Insert(0, request); else Requests[existing.index] = request;
    }
    private IReadOnlyList<string> ParseRequiredFields() => RequiredFields.Split([',', '،', ';', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct(StringComparer.Ordinal).ToArray();
    private static bool TryParseState(string? value, out RegistrationState? state)
    {
        state = value switch { "" or null => null, "pending" => RegistrationState.Pending, "completion_requested" => RegistrationState.CompletionRequested, "accepted" => RegistrationState.Accepted, "rejected" => RegistrationState.Rejected, "withdrawn" => RegistrationState.Withdrawn, "cancelled" => RegistrationState.Cancelled, _ => null };
        return string.IsNullOrEmpty(value) || state is not null;
    }
    private void ClearFeedback() { IsError = false; Message = null; }
    private void SetLocalFailure(string message) { ClearFeedback(); IsError = true; Message = message; }
    private void SetFailure(AppError? error) { IsError = true; Message = error?.Message ?? "تعذر إتمام العملية. أعد المحاولة."; }
    private void NotifyCommands()
    {
        LoadCommand.NotifyCanExecuteChanged(); ApplyFilterCommand.NotifyCanExecuteChanged(); LoadPreviousPageCommand.NotifyCanExecuteChanged(); LoadNextPageCommand.NotifyCanExecuteChanged(); AcceptCommand.NotifyCanExecuteChanged(); RejectCommand.NotifyCanExecuteChanged(); RequestCompletionCommand.NotifyCanExecuteChanged();
    }
    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
