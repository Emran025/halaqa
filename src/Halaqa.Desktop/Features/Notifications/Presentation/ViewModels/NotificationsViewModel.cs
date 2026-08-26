using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Notifications.Domain.Entities;
using Halaqa.Desktop.Features.Notifications.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Notifications.Presentation.ViewModels;

public sealed partial class NotificationsViewModel : ObservableObject
{
    private const int PageSize = 20;
    private readonly ListNotificationsUseCase _listNotificationsUseCase;
    private readonly MarkNotificationReadUseCase _markNotificationReadUseCase;
    private readonly MarkAllNotificationsReadUseCase _markAllNotificationsReadUseCase;

    public NotificationsViewModel(
        ListNotificationsUseCase listNotificationsUseCase,
        MarkNotificationReadUseCase markNotificationReadUseCase,
        MarkAllNotificationsReadUseCase markAllNotificationsReadUseCase)
    {
        _listNotificationsUseCase = listNotificationsUseCase;
        _markNotificationReadUseCase = markNotificationReadUseCase;
        _markAllNotificationsReadUseCase = markAllNotificationsReadUseCase;
    }

    public ObservableCollection<HalaqaNotification> Notifications { get; } = [];

    [ObservableProperty] private HalaqaNotification? _selectedNotification;
    [ObservableProperty] private bool _unreadOnly;
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _lastPage = 1;
    [ObservableProperty] private int _total;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private string? _message;

    public event EventHandler? BackRequested;

    public void Initialize()
    {
        Notifications.Clear();
        SelectedNotification = null;
        UnreadOnly = false;
        CurrentPage = 1;
        LastPage = 1;
        Total = 0;
        ClearFeedback();
    }

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadAsync() => await LoadPageAsync(1);

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task RefreshAsync() => await LoadPageAsync(CurrentPage);

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task ToggleUnreadOnlyAsync()
    {
        UnreadOnly = !UnreadOnly;
        await LoadPageAsync(1);
    }

    [RelayCommand(CanExecute = nameof(CanLoadPrevious))]
    private async Task LoadPreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            await LoadPageAsync(CurrentPage - 1);
        }
    }

    [RelayCommand(CanExecute = nameof(CanLoadNext))]
    private async Task LoadNextPageAsync()
    {
        if (CurrentPage < LastPage)
        {
            await LoadPageAsync(CurrentPage + 1);
        }
    }

    [RelayCommand(CanExecute = nameof(CanMarkSelectedRead))]
    private async Task MarkSelectedReadAsync()
    {
        var notification = SelectedNotification;
        if (notification is null || notification.IsRead)
        {
            return;
        }

        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _markNotificationReadUseCase.ExecuteAsync(notification.Id);
            if (!result.IsSuccess)
            {
                SetFailure(result.Error);
                return;
            }

            Message = "تم تعليم الإشعار كمقروء.";
            await LoadPageAsync(CurrentPage, keepBusy: true);
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanMarkAllRead))]
    private async Task MarkAllReadAsync()
    {
        IsBusy = true;
        ClearFeedback();
        try
        {
            var result = await _markAllNotificationsReadUseCase.ExecuteAsync();
            if (!result.IsSuccess)
            {
                SetFailure(result.Error);
                return;
            }

            Message = "تم تعليم جميع الإشعارات كمقروءة.";
            await LoadPageAsync(CurrentPage, keepBusy: true);
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    private async Task LoadPageAsync(int page, bool keepBusy = false)
    {
        if (!keepBusy)
        {
            IsBusy = true;
            ClearFeedback();
        }

        try
        {
            var result = await _listNotificationsUseCase.ExecuteAsync(new NotificationQuery(UnreadOnly, page, PageSize));
            if (!result.IsSuccess || result.Value is null)
            {
                SetFailure(result.Error);
                return;
            }

            Notifications.Clear();
            foreach (var notification in result.Value.Notifications)
            {
                Notifications.Add(notification);
            }
            CurrentPage = result.Value.CurrentPage;
            LastPage = result.Value.LastPage;
            Total = result.Value.Total;
            SelectedNotification = Notifications.FirstOrDefault();
        }
        finally
        {
            if (!keepBusy)
            {
                IsBusy = false;
                NotifyCommands();
            }
        }
    }

    private bool CanLoad() => !IsBusy;
    private bool CanLoadPrevious() => CanLoad() && CurrentPage > 1;
    private bool CanLoadNext() => CanLoad() && CurrentPage < LastPage;
    private bool CanMarkSelectedRead() => CanLoad() && SelectedNotification is { IsRead: false };
    private bool CanMarkAllRead() => CanLoad() && Notifications.Any(notification => !notification.IsRead);

    private void ClearFeedback()
    {
        IsError = false;
        Message = null;
    }

    private void SetFailure(AppError? error)
    {
        IsError = true;
        Message = error?.Message ?? "تعذر تنفيذ عملية الإشعارات.";
    }

    private void NotifyCommands()
    {
        LoadCommand.NotifyCanExecuteChanged();
        RefreshCommand.NotifyCanExecuteChanged();
        ToggleUnreadOnlyCommand.NotifyCanExecuteChanged();
        LoadPreviousPageCommand.NotifyCanExecuteChanged();
        LoadNextPageCommand.NotifyCanExecuteChanged();
        MarkSelectedReadCommand.NotifyCanExecuteChanged();
        MarkAllReadCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedNotificationChanged(HalaqaNotification? value) => NotifyCommands();
}
