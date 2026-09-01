using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.TeacherDocuments.Domain.Entities;
using Halaqa.Desktop.Features.TeacherDocuments.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;
using Microsoft.Win32;

namespace Halaqa.Desktop.Features.TeacherDocuments.Presentation.ViewModels;

public sealed partial class TeacherDocumentsViewModel : ObservableObject
{
    private readonly ListTeacherDocumentsUseCase _listTeacherDocumentsUseCase;
    private readonly CreateTeacherDocumentUseCase _createTeacherDocumentUseCase;
    private readonly DeleteTeacherDocumentUseCase _deleteTeacherDocumentUseCase;
    private TeacherDocumentFile? _selectedFile;

    public TeacherDocumentsViewModel(
        ListTeacherDocumentsUseCase listTeacherDocumentsUseCase,
        CreateTeacherDocumentUseCase createTeacherDocumentUseCase,
        DeleteTeacherDocumentUseCase deleteTeacherDocumentUseCase)
    {
        _listTeacherDocumentsUseCase = listTeacherDocumentsUseCase;
        _createTeacherDocumentUseCase = createTeacherDocumentUseCase;
        _deleteTeacherDocumentUseCase = deleteTeacherDocumentUseCase;
    }

    public ObservableCollection<TeacherDocument> Documents { get; } = new();

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _certificateType = string.Empty;
    [ObservableProperty] private string? _certificateTypeOther;
    [ObservableProperty] private string? _riwayah;
    [ObservableProperty] private string? _issuingPlace;
    [ObservableProperty] private string? _issuingDate;
    [ObservableProperty] private string? _selectedFileName;
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _lastPage = 1;
    [ObservableProperty] private int _total;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private string? _nameError;
    [ObservableProperty] private string? _certificateTypeError;
    [ObservableProperty] private string? _certificateTypeOtherError;
    [ObservableProperty] private string? _riwayahError;
    [ObservableProperty] private string? _issuingPlaceError;
    [ObservableProperty] private string? _issuingDateError;
    [ObservableProperty] private string? _fileError;
    [ObservableProperty] private bool _isDialogOpen;

    public event EventHandler? BackRequested;

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadAsync()
    {
        await LoadPageAsync(1);
    }

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
    private void OpenUploadDialog()
    {
        ClearForm();
        ClearFeedback();
        IsDialogOpen = true;
    }

    [RelayCommand]
    private void CloseDialog()
    {
        IsDialogOpen = false;
        ClearFeedback();
    }

    [RelayCommand(CanExecute = nameof(CanChooseFile))]
    private void ChooseFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "اختيار ملف الوثيقة",
            CheckFileExists = true,
            Multiselect = false
        };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var content = File.ReadAllBytes(dialog.FileName);
            _selectedFile = new TeacherDocumentFile(
                Path.GetFileName(dialog.FileName),
                GuessContentType(Path.GetExtension(dialog.FileName)),
                content);
            SelectedFileName = _selectedFile.FileName;
            FileError = null;
        }
        catch (IOException)
        {
            _selectedFile = null;
            SelectedFileName = null;
            FileError = "تعذر قراءة ملف الوثيقة المحدد.";
        }
        catch (UnauthorizedAccessException)
        {
            _selectedFile = null;
            SelectedFileName = null;
            FileError = "لا تملك صلاحية قراءة ملف الوثيقة المحدد.";
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (!TryCreateCommand(out var command, out var localError))
        {
            SetLocalFailure(localError!);
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _createTeacherDocumentUseCase.ExecuteAsync(command!);
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            Documents.Insert(0, result.Value);
            Total++;
            ClearForm();
            Message = "تم حفظ وثيقة المعلم بنجاح.";
            IsDialogOpen = false;
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteAsync(TeacherDocument? document)
    {
        if (document is null)
        {
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _deleteTeacherDocumentUseCase.ExecuteAsync(document.Id);
            if (!result.IsSuccess)
            {
                SetFailure(result.Error);
                return;
            }

            Documents.Remove(document);
            Total = Math.Max(0, Total - 1);
            Message = "تم حذف الوثيقة.";
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    partial void OnNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnCertificateTypeChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnCertificateTypeOtherChanged(string? value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnRiwayahChanged(string? value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnIssuingPlaceChanged(string? value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnIssuingDateChanged(string? value) => SaveCommand.NotifyCanExecuteChanged();

    private bool CanLoad() => !IsBusy;
    private bool CanSave() => !IsBusy && !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(CertificateType);
    private bool CanChooseFile() => !IsBusy;
    private bool CanDelete(TeacherDocument? document) => !IsBusy && document is not null;
    private bool CanNavigateBack() => !IsBusy;

    private async Task LoadPageAsync(int page)
    {
        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _listTeacherDocumentsUseCase.ExecuteAsync(page);
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            Documents.Clear();
            foreach (var document in result.Value.Documents)
            {
                Documents.Add(document);
            }
            CurrentPage = result.Value.CurrentPage;
            LastPage = result.Value.LastPage;
            Total = result.Value.Total;
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    private bool TryCreateCommand(out CreateTeacherDocumentCommand? command, out string? error)
    {
        command = null;
        error = null;
        DateOnly? date = null;
        if (!string.IsNullOrWhiteSpace(IssuingDate) &&
            !DateOnly.TryParseExact(IssuingDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedDate))
        {
            error = "أدخل تاريخ الإصدار بصيغة YYYY-MM-DD.";
            return false;
        }
        if (!string.IsNullOrWhiteSpace(IssuingDate))
        {
            date = DateOnly.ParseExact(IssuingDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        }

        command = new CreateTeacherDocumentCommand(
            Name.Trim(),
            CertificateType.Trim(),
            NormalizeOptional(CertificateTypeOther),
            NormalizeOptional(Riwayah),
            NormalizeOptional(IssuingPlace),
            date,
            _selectedFile);
        return true;
    }

    private void ClearForm()
    {
        Name = string.Empty;
        CertificateType = string.Empty;
        CertificateTypeOther = null;
        Riwayah = null;
        IssuingPlace = null;
        IssuingDate = null;
        _selectedFile = null;
        SelectedFileName = null;
    }

    private void NotifyCommands()
    {
        LoadCommand.NotifyCanExecuteChanged();
        LoadNextPageCommand.NotifyCanExecuteChanged();
        LoadPreviousPageCommand.NotifyCanExecuteChanged();
        ChooseFileCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
        BackCommand.NotifyCanExecuteChanged();
    }

    private void ClearFeedback()
    {
        IsError = false;
        Message = null;
        NameError = null;
        CertificateTypeError = null;
        CertificateTypeOtherError = null;
        RiwayahError = null;
        IssuingPlaceError = null;
        IssuingDateError = null;
        FileError = null;
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
                    case "name": NameError = fieldMessage; break;
                    case "certificate_type": CertificateTypeError = fieldMessage; break;
                    case "certificate_type_other": CertificateTypeOtherError = fieldMessage; break;
                    case "riwayah": RiwayahError = fieldMessage; break;
                    case "issuing_place": IssuingPlaceError = fieldMessage; break;
                    case "issuing_date": IssuingDateError = fieldMessage; break;
                    case "file": FileError = fieldMessage; break;
                }
            }
        }
        Message = error?.Message ?? "تعذر إتمام العملية. أعد المحاولة.";
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string GuessContentType(string extension) => extension.ToLowerInvariant() switch
    {
        ".pdf" => "application/pdf",
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        _ => "application/octet-stream"
    };
}
