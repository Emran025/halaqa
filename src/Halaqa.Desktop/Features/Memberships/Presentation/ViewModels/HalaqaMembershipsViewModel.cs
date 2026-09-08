using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Memberships.Domain.Entities;
using Halaqa.Desktop.Features.Memberships.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;
using Halaqa.Desktop.Shared.Presentation.Models;

namespace Halaqa.Desktop.Features.Memberships.Presentation.ViewModels;

public sealed partial class HalaqaMembershipsViewModel : ObservableObject
{
    private readonly ListHalaqaMembershipsUseCase _listHalaqaMembershipsUseCase;
    private readonly AssignStudentToHalaqaUseCase _assignStudentToHalaqaUseCase;
    private readonly UpdateHalaqaMembershipUseCase _updateHalaqaMembershipUseCase;
    private readonly RemoveHalaqaMembershipUseCase _removeHalaqaMembershipUseCase;
    private Guid _halaqaId;

    private readonly List<HalaqaMembership> _rawMemberships = new();

    public HalaqaMembershipsViewModel(
        ListHalaqaMembershipsUseCase listHalaqaMembershipsUseCase,
        AssignStudentToHalaqaUseCase assignStudentToHalaqaUseCase,
        UpdateHalaqaMembershipUseCase updateHalaqaMembershipUseCase,
        RemoveHalaqaMembershipUseCase removeHalaqaMembershipUseCase)
    {
        _listHalaqaMembershipsUseCase = listHalaqaMembershipsUseCase;
        _assignStudentToHalaqaUseCase = assignStudentToHalaqaUseCase;
        _updateHalaqaMembershipUseCase = updateHalaqaMembershipUseCase;
        _removeHalaqaMembershipUseCase = removeHalaqaMembershipUseCase;
    }

    public ObservableCollection<HalaqaMembership> Memberships { get; } = new();

    public IReadOnlyList<LocalizedOption<string>> MembershipStatusOptions { get; } = new[]
    {
        new LocalizedOption<string>("active", "نشطة"),
        new LocalizedOption<string>("inactive", "غير نشطة")
    };

    public IReadOnlyList<LocalizedOption<string>> FilterOptions { get; } = new[]
    {
        new LocalizedOption<string>(string.Empty, "كل العضويات"),
        new LocalizedOption<string>("active", "نشطة"),
        new LocalizedOption<string>("inactive", "غير نشطة"),
        new LocalizedOption<string>("removed", "مزالة")
    };

    [ObservableProperty] private string _halaqaName = string.Empty;
    [ObservableProperty] private HalaqaMembership? _selectedMembership;
    [ObservableProperty] private string _studentId = string.Empty;
    [ObservableProperty] private string _selectedStatus = "active";
    [ObservableProperty] private string? _reason;
    [ObservableProperty] private string _filterStatus = string.Empty;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private bool _isDialogOpen;
    [ObservableProperty] private bool _isAssignMode;
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _lastPage = 1;
    [ObservableProperty] private int _total;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private string? _studentIdError;
    [ObservableProperty] private string? _statusError;
    [ObservableProperty] private string? _reasonError;

    public bool HasNoMemberships => Memberships.Count == 0 && !IsBusy;

    public string DialogTitle => IsAssignMode
        ? "إسناد طالب جديد إلى الحلقة"
        : (SelectedMembership is null ? "إدارة العضوية" : $"إدارة عضوية: {SelectedMembership.Student.Name}");

    public string EditorTitle => DialogTitle;

    public event EventHandler? BackRequested;

    public void Initialize(Guid halaqaId, string halaqaName)
    {
        _halaqaId = halaqaId;
        HalaqaName = halaqaName;
        SelectedMembership = null;
        StudentId = string.Empty;
        SelectedStatus = "active";
        Reason = null;
        FilterStatus = string.Empty;
        SearchText = string.Empty;
        IsDialogOpen = false;
        IsAssignMode = false;
        _rawMemberships.Clear();
        Memberships.Clear();
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

    [RelayCommand]
    private void OpenAssignDialog()
    {
        IsAssignMode = true;
        SelectedMembership = null;
        StudentId = string.Empty;
        ClearFeedback();
        OnPropertyChanged(nameof(DialogTitle));
        OnPropertyChanged(nameof(EditorTitle));
        IsDialogOpen = true;
        NotifyCommands();
    }

    [RelayCommand]
    private void OpenManageDialog(HalaqaMembership? membership)
    {
        if (membership is not null)
        {
            SelectedMembership = membership;
            SelectedStatus = ToContractValue(membership.Status);
            Reason = null;
        }
        IsAssignMode = false;
        ClearFeedback();
        OnPropertyChanged(nameof(DialogTitle));
        OnPropertyChanged(nameof(EditorTitle));
        IsDialogOpen = true;
        NotifyCommands();
    }

    [RelayCommand]
    private void CloseDialog()
    {
        IsDialogOpen = false;
        ClearFeedback();
    }

    [RelayCommand(CanExecute = nameof(CanAssign))]
    private async Task AssignAsync()
    {
        if (!Guid.TryParse(StudentId, out var parsedStudentId))
        {
            SetLocalFailure("أدخل معرّف الطالب بصيغة UUID صحيحة.");
            StudentIdError = "معرّف الطالب غير صالح.";
            return;
        }
        if (!EnsureHalaqaSelected())
        {
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _assignStudentToHalaqaUseCase.ExecuteAsync(new AssignStudentToHalaqaCommand(_halaqaId, parsedStudentId));
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            Upsert(result.Value);
            StudentId = string.Empty;
            Total++;
            Message = "تم إسناد الطالب إلى الحلقة بنجاح.";
            IsDialogOpen = false;
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private async Task UpdateStatusAsync()
    {
        var selected = SelectedMembership;
        if (selected is null || !EnsureHalaqaSelected())
        {
            return;
        }
        if (!Enum.TryParse<MembershipStatus>(SelectedStatus, ignoreCase: true, out var status) || status == MembershipStatus.Removed)
        {
            SetLocalFailure("اختر حالة عضوية صحيحة.");
            StatusError = "الحالة غير صالحة.";
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _updateHalaqaMembershipUseCase.ExecuteAsync(new UpdateHalaqaMembershipCommand(
                _halaqaId,
                selected.Id,
                status,
                NormalizeOptional(Reason)));
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            Upsert(result.Value);
            SelectedMembership = result.Value;
            Message = "تم تحديث حالة العضوية بنجاح.";
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanRemove))]
    private async Task RemoveAsync()
    {
        var selected = SelectedMembership;
        if (selected is null || !EnsureHalaqaSelected())
        {
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _removeHalaqaMembershipUseCase.ExecuteAsync(_halaqaId, selected.Id);
            if (!result.IsSuccess)
            {
                SetFailure(result.Error);
                return;
            }

            var index = _rawMemberships.FindIndex(m => m.Id == selected.Id);
            if (index >= 0)
            {
                _rawMemberships.RemoveAt(index);
            }
            ApplyLocalFilter();
            SelectedMembership = null;
            Total = Math.Max(0, Total - 1);
            Message = "تمت إزالة العضوية مع الإبقاء على سجلها التاريخي.";
            IsDialogOpen = false;
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    partial void OnSearchTextChanged(string value) => ApplyLocalFilter();

    partial void OnSelectedMembershipChanged(HalaqaMembership? value)
    {
        if (value is not null)
        {
            SelectedStatus = ToContractValue(value.Status);
            Reason = null;
        }
        OnPropertyChanged(nameof(DialogTitle));
        OnPropertyChanged(nameof(EditorTitle));
        UpdateStatusCommand.NotifyCanExecuteChanged();
        RemoveCommand.NotifyCanExecuteChanged();
    }

    partial void OnStudentIdChanged(string value) => AssignCommand.NotifyCanExecuteChanged();
    partial void OnSelectedStatusChanged(string value) => UpdateStatusCommand.NotifyCanExecuteChanged();

    private bool CanLoad() => !IsBusy && _halaqaId != Guid.Empty;
    private bool CanAssign() => !IsBusy && _halaqaId != Guid.Empty && !string.IsNullOrWhiteSpace(StudentId);
    private bool CanUpdate() => !IsBusy && _halaqaId != Guid.Empty && SelectedMembership is not null;
    private bool CanRemove() => !IsBusy && _halaqaId != Guid.Empty && SelectedMembership is not null;
    private bool CanNavigateBack() => !IsBusy;

    private async Task LoadPageAsync(int page)
    {
        if (!EnsureHalaqaSelected())
        {
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _listHalaqaMembershipsUseCase.ExecuteAsync(_halaqaId, NormalizeOptional(FilterStatus), page);
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            _rawMemberships.Clear();
            foreach (var membership in result.Value.Memberships)
            {
                _rawMemberships.Add(membership);
            }
            ApplyLocalFilter();
            CurrentPage = result.Value.CurrentPage;
            LastPage = result.Value.LastPage;
            Total = result.Value.Total;
            SelectedMembership = null;
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    private void Upsert(HalaqaMembership membership)
    {
        var rawIndex = _rawMemberships.FindIndex(m => m.Id == membership.Id);
        if (rawIndex >= 0)
        {
            _rawMemberships[rawIndex] = membership;
        }
        else
        {
            _rawMemberships.Insert(0, membership);
        }
        ApplyLocalFilter();
    }

    private void ApplyLocalFilter()
    {
        Memberships.Clear();
        var query = _rawMemberships.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var term = SearchText.Trim();
            query = query.Where(m =>
                (m.Student.Name != null && m.Student.Name.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                (m.Student.Email != null && m.Student.Email.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                (m.Student.Phone != null && m.Student.Phone.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        foreach (var m in query)
        {
            Memberships.Add(m);
        }
        OnPropertyChanged(nameof(HasNoMemberships));
    }

    private bool EnsureHalaqaSelected()
    {
        if (_halaqaId != Guid.Empty)
        {
            return true;
        }

        SetLocalFailure("اختر حلقة أولاً قبل إدارة عضوياتها.");
        return false;
    }

    private void NotifyCommands()
    {
        LoadCommand.NotifyCanExecuteChanged();
        ApplyFilterCommand.NotifyCanExecuteChanged();
        LoadNextPageCommand.NotifyCanExecuteChanged();
        LoadPreviousPageCommand.NotifyCanExecuteChanged();
        AssignCommand.NotifyCanExecuteChanged();
        UpdateStatusCommand.NotifyCanExecuteChanged();
        RemoveCommand.NotifyCanExecuteChanged();
        BackCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(HasNoMemberships));
    }

    private void ClearFeedback()
    {
        IsError = false;
        Message = null;
        StudentIdError = null;
        StatusError = null;
        ReasonError = null;
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
                    case "student_id": StudentIdError = fieldMessage; break;
                    case "status": StatusError = fieldMessage; break;
                    case "reason": ReasonError = fieldMessage; break;
                }
            }
        }
        Message = error?.Message ?? "تعذر إتمام العملية. أعد المحاولة.";
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string ToContractValue<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var name = value.ToString();
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}
