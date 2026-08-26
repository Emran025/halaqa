using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Registrations.Domain.Entities;
using Halaqa.Desktop.Features.Registrations.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Registrations.Presentation.ViewModels;

public sealed partial class HalaqaRegistrationRequestsViewModel : ObservableObject
{
    private readonly ListHalaqaRegistrationRequestsUseCase _listRequestsUseCase;
    private readonly AcceptRegistrationRequestUseCase _acceptRequestUseCase;
    private readonly RejectRegistrationRequestUseCase _rejectRequestUseCase;
    private readonly RequestRegistrationCompletionUseCase _requestCompletionUseCase;
    private Guid _halaqaId;

    public HalaqaRegistrationRequestsViewModel(
        ListHalaqaRegistrationRequestsUseCase listRequestsUseCase,
        AcceptRegistrationRequestUseCase acceptRequestUseCase,
        RejectRegistrationRequestUseCase rejectRequestUseCase,
        RequestRegistrationCompletionUseCase requestCompletionUseCase)
    {
        _listRequestsUseCase = listRequestsUseCase;
        _acceptRequestUseCase = acceptRequestUseCase;
        _rejectRequestUseCase = rejectRequestUseCase;
        _requestCompletionUseCase = requestCompletionUseCase;
    }

    public ObservableCollection<RegistrationRequest> Requests { get; } = new();
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

    [ObservableProperty] private string _halaqaName = string.Empty;
    [ObservableProperty] private RegistrationRequest? _selectedRequest;
    [ObservableProperty] private string _filterState = string.Empty;
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

    public string EditorTitle => SelectedRequest is null
        ? "اختر طلب تسجيل لمراجعته"
        : $"مراجعة طلب {SelectedRequest.Applicant.DisplayName}";

    public event EventHandler? BackRequested;

    public void Initialize(Guid halaqaId, string halaqaName)
    {
        _halaqaId = halaqaId;
        HalaqaName = halaqaName;
        SelectedRequest = null;
        FilterState = string.Empty;
        RejectionNote = null;
        RequiredFields = string.Empty;
        CompletionNote = null;
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

    [RelayCommand(CanExecute = nameof(CanDecide))]
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
            var result = await _acceptRequestUseCase.ExecuteAsync(selected.Id);
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            Upsert(result.Value);
            SelectedRequest = result.Value;
            Message = "تم قبول طلب التسجيل. تظهر العضوية الناتجة وفق إجراءات الخادم.";
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanDecide))]
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

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    partial void OnSelectedRequestChanged(RegistrationRequest? value)
    {
        RejectionNote = null;
        RequiredFields = string.Empty;
        CompletionNote = null;
        OnPropertyChanged(nameof(EditorTitle));
        AcceptCommand.NotifyCanExecuteChanged();
        RejectCommand.NotifyCanExecuteChanged();
        RequestCompletionCommand.NotifyCanExecuteChanged();
    }

    partial void OnRequiredFieldsChanged(string value) => RequestCompletionCommand.NotifyCanExecuteChanged();

    private bool CanLoad() => !IsBusy && _halaqaId != Guid.Empty;
    private bool CanDecide() => !IsBusy && SelectedRequest is not null;
    private bool CanRequestCompletion() => !IsBusy && SelectedRequest is not null && ParseRequiredFields().Count > 0;
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

    private void Upsert(RegistrationRequest request)
    {
        var existing = Requests.Select((value, index) => (value, index)).FirstOrDefault(item => item.value.Id == request.Id);
        if (existing.value is not null)
        {
            Requests[existing.index] = request;
        }
        else
        {
            Requests.Insert(0, request);
        }
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
        LoadCommand.NotifyCanExecuteChanged();
        ApplyFilterCommand.NotifyCanExecuteChanged();
        LoadNextPageCommand.NotifyCanExecuteChanged();
        LoadPreviousPageCommand.NotifyCanExecuteChanged();
        AcceptCommand.NotifyCanExecuteChanged();
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
