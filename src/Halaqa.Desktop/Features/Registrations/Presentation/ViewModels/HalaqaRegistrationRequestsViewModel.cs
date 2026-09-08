using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Memberships.Domain.Entities;
using Halaqa.Desktop.Features.Memberships.Domain.UseCases;
using Halaqa.Desktop.Features.Registrations.Domain.Entities;
using Halaqa.Desktop.Features.Registrations.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;
using Halaqa.Desktop.Shared.Presentation.Models;

namespace Halaqa.Desktop.Features.Registrations.Presentation.ViewModels;

public sealed partial class HalaqaRegistrationRequestsViewModel : ObservableObject
{
    private readonly ListHalaqaRegistrationRequestsUseCase _listRequestsUseCase;
    private readonly AcceptRegistrationRequestUseCase _acceptRequestUseCase;
    private readonly RejectRegistrationRequestUseCase _rejectRequestUseCase;
    private readonly RequestRegistrationCompletionUseCase _requestCompletionUseCase;
    private readonly AssignStudentToHalaqaUseCase _assignStudentToHalaqaUseCase;
    private Guid _halaqaId;

    public HalaqaRegistrationRequestsViewModel(
        ListHalaqaRegistrationRequestsUseCase listRequestsUseCase,
        AcceptRegistrationRequestUseCase acceptRequestUseCase,
        RejectRegistrationRequestUseCase rejectRequestUseCase,
        RequestRegistrationCompletionUseCase requestCompletionUseCase,
        AssignStudentToHalaqaUseCase assignStudentToHalaqaUseCase)
    {
        _listRequestsUseCase = listRequestsUseCase;
        _acceptRequestUseCase = acceptRequestUseCase;
        _rejectRequestUseCase = rejectRequestUseCase;
        _requestCompletionUseCase = requestCompletionUseCase;
        _assignStudentToHalaqaUseCase = assignStudentToHalaqaUseCase;
    }

    private readonly List<RegistrationRequest> _rawRequests = new();

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

    [ObservableProperty] private string _halaqaName = string.Empty;
    [ObservableProperty] private RegistrationRequest? _selectedRequest;
    [ObservableProperty] private string _filterState = string.Empty;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private bool _isDialogOpen;
    [ObservableProperty] private string? _rejectionNote;
    [ObservableProperty] private string _requiredFields = string.Empty;
    [ObservableProperty] private string? _completionNote;
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _lastPage = 1;
    [ObservableProperty] private int _total;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private string? _rejectionNoteError;
    [ObservableProperty] private string? _requiredFieldsError;
    [ObservableProperty] private string? _completionNoteError;

    public bool HasNoRequests => Requests.Count == 0 && !IsBusy;

    public string EditorTitle => SelectedRequest is null
        ? "مراجعة طلب التسجيل"
        : $"مراجعة طلب: {SelectedRequest.Applicant.DisplayName}";

    public event EventHandler? BackRequested;

    public void Initialize(Guid halaqaId, string halaqaName)
    {
        _halaqaId = halaqaId;
        HalaqaName = halaqaName;
        SelectedRequest = null;
        FilterState = string.Empty;
        SearchText = string.Empty;
        IsDialogOpen = false;
        RejectionNote = null;
        RequiredFields = string.Empty;
        CompletionNote = null;
        _rawRequests.Clear();
        Requests.Clear();
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

    public bool IsPendingState => SelectedRequest is not null && (SelectedRequest.State == RegistrationState.Pending || SelectedRequest.State == RegistrationState.CompletionRequested);
    public bool IsAcceptedState => SelectedRequest is not null && SelectedRequest.State == RegistrationState.Accepted;

    [RelayCommand(CanExecute = nameof(CanAccept))]
    private async Task AcceptAsync()
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
            var result = await _acceptRequestUseCase.ExecuteAsync(selected.Id, _halaqaId);
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            Upsert(result.Value);
            SelectedRequest = result.Value;
            Message = "تم قبول طلب التسجيل وإدخال الطالب إلى الحلقة بنجاح.";
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanAssignToHalaqa))]
    private async Task AssignToHalaqaAsync()
    {
        var selected = SelectedRequest;
        if (selected is null || _halaqaId == Guid.Empty)
        {
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _assignStudentToHalaqaUseCase.ExecuteAsync(
                new AssignStudentToHalaqaCommand(_halaqaId, selected.Applicant.Id));

            if (!result.IsSuccess)
            {
                SetFailure(result.Error);
                return;
            }

            Message = "تم إدخال الطالب إلى هذه الحلقة بنجاح.";
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanReject))]
    private async Task RejectAsync()
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
            var result = await _rejectRequestUseCase.ExecuteAsync(new RejectRegistrationRequestCommand(
                selected.Id,
                NormalizeOptional(RejectionNote)));
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            Upsert(result.Value);
            SelectedRequest = result.Value;
            RejectionNote = null;
            Message = "تم رفض طلب التسجيل.";
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanRequestCompletion))]
    private async Task RequestCompletionAsync()
    {
        var selected = SelectedRequest;
        var fields = ParseRequiredFields();
        if (selected is null)
        {
            return;
        }
        if (fields.Count == 0)
        {
            SetLocalFailure("أضف حقلاً واحداً على الأقل لطلب الاستكمال.");
            RequiredFieldsError = "أدخل حقلاً واحداً على الأقل.";
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _requestCompletionUseCase.ExecuteAsync(new RequestRegistrationCompletionCommand(
                selected.Id,
                fields,
                NormalizeOptional(CompletionNote)));
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            Upsert(result.Value);
            SelectedRequest = result.Value;
            RequiredFields = string.Empty;
            CompletionNote = null;
            Message = "تم إرسال طلب استكمال البيانات.";
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand]
    private void OpenDialog(RegistrationRequest? request)
    {
        if (request is not null)
        {
            SelectedRequest = request;
        }
        ClearFeedback();
        IsDialogOpen = true;
    }

    [RelayCommand]
    private void CloseDialog()
    {
        IsDialogOpen = false;
    }

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    partial void OnSearchTextChanged(string value) => ApplyLocalFilter();

    partial void OnSelectedRequestChanged(RegistrationRequest? value)
    {
        RejectionNote = null;
        RequiredFields = string.Empty;
        CompletionNote = null;
        OnPropertyChanged(nameof(EditorTitle));
        NotifyCommands();
    }

    partial void OnRequiredFieldsChanged(string value) => RequestCompletionCommand.NotifyCanExecuteChanged();

    private bool CanLoad() => !IsBusy && _halaqaId != Guid.Empty;
    private bool CanAccept() => !IsBusy && IsPendingState;
    private bool CanAssignToHalaqa() => !IsBusy && _halaqaId != Guid.Empty && IsAcceptedState;
    private bool CanReject() => !IsBusy && IsPendingState;
    private bool CanRequestCompletion() => !IsBusy && IsPendingState && ParseRequiredFields().Count > 0;
    private bool CanNavigateBack() => !IsBusy;

    private async Task LoadPageAsync(int page)
    {
        if (!EnsureHalaqaSelected())
        {
            return;
        }

        if (!TryParseState(FilterState, out var state))
        {
            SetLocalFailure("اختر حالة طلب صحيحة.");
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _listRequestsUseCase.ExecuteAsync(_halaqaId, state, page);
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            _rawRequests.Clear();
            foreach (var request in result.Value.Requests)
            {
                _rawRequests.Add(request);
            }
            ApplyLocalFilter();
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

    private void Upsert(RegistrationRequest request)
    {
        var rawIndex = _rawRequests.FindIndex(r => r.Id == request.Id);
        if (rawIndex >= 0)
        {
            _rawRequests[rawIndex] = request;
        }
        else
        {
            _rawRequests.Insert(0, request);
        }
        ApplyLocalFilter();
    }

    private void ApplyLocalFilter()
    {
        Requests.Clear();
        var query = _rawRequests.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var term = SearchText.Trim();
            query = query.Where(r =>
                r.Applicant.DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (r.Message != null && r.Message.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        foreach (var r in query)
        {
            Requests.Add(r);
        }

        OnPropertyChanged(nameof(HasNoRequests));
    }

    private IReadOnlyList<string> ParseRequiredFields() => RequiredFields
        .Split(new[] { ',', '،', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Distinct(StringComparer.Ordinal)
        .ToArray();

    private bool EnsureHalaqaSelected()
    {
        if (_halaqaId != Guid.Empty)
        {
            return true;
        }

        SetLocalFailure("اختر حلقة أولاً قبل مراجعة طلبات التسجيل.");
        return false;
    }

    private void NotifyCommands()
    {
        OnPropertyChanged(nameof(IsPendingState));
        OnPropertyChanged(nameof(IsAcceptedState));
        OnPropertyChanged(nameof(HasNoRequests));
        LoadCommand.NotifyCanExecuteChanged();
        ApplyFilterCommand.NotifyCanExecuteChanged();
        LoadNextPageCommand.NotifyCanExecuteChanged();
        LoadPreviousPageCommand.NotifyCanExecuteChanged();
        AcceptCommand.NotifyCanExecuteChanged();
        AssignToHalaqaCommand.NotifyCanExecuteChanged();
        RejectCommand.NotifyCanExecuteChanged();
        RequestCompletionCommand.NotifyCanExecuteChanged();
        BackCommand.NotifyCanExecuteChanged();
    }

    private void ClearFeedback()
    {
        IsError = false;
        Message = null;
        RejectionNoteError = null;
        RequiredFieldsError = null;
        CompletionNoteError = null;
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
        if (error?.FieldErrors is { Count: > 0 } fieldErrors)
        {
            foreach (var fieldError in fieldErrors)
            {
                var fieldMessage = string.Join(" ", fieldError.Messages);
                switch (fieldError.Field)
                {
                    case "note":
                        RejectionNoteError = fieldMessage;
                        CompletionNoteError = fieldMessage;
                        break;
                    case "required_fields":
                        RequiredFieldsError = fieldMessage;
                        break;
                }
            }
        }
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

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
