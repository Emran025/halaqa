using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Progress.Domain.Entities;
using Halaqa.Desktop.Features.Progress.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Progress.Presentation.ViewModels;

public sealed partial class StudentProgressViewModel : ObservableObject
{
    private readonly GetStudentProgressUseCase getStudentProgressUseCase;

    public StudentProgressViewModel(GetStudentProgressUseCase getStudentProgressUseCase)
    {
        this.getStudentProgressUseCase = getStudentProgressUseCase;
    }

    public IReadOnlyList<string> TaskTypeOptions { get; } = new[] { "", "memorization", "review", "recitation" };

    [ObservableProperty] private Guid _studentId;
    [ObservableProperty] private string _selectedTaskType = string.Empty;
    [ObservableProperty] private StudentProgress? _progress;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private string? _message;

    public event EventHandler? BackRequested;

    public void Initialize(Guid studentId)
    {
        StudentId = studentId;
        SelectedTaskType = string.Empty;
        Progress = null;
        ClearFeedback();
        LoadCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadAsync()
    {
        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await getStudentProgressUseCase.ExecuteAsync(StudentId, string.IsNullOrWhiteSpace(SelectedTaskType) ? null : SelectedTaskType);
            if (!result.IsSuccess || result.Value is null)
            {
                IsError = true;
                Message = result.Error?.Message ?? "تعذر تحميل تقدم الطالب.";
                return;
            }
            Progress = result.Value;
        }
        finally
        {
            IsBusy = false;
            LoadCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    private bool CanLoad() => !IsBusy && StudentId != Guid.Empty;

    private void ClearFeedback()
    {
        IsError = false;
        Message = null;
    }
}
