using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Quran.Domain.Entities;
using Halaqa.Desktop.Features.Quran.Domain.UseCases;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;
using Halaqa.Desktop.Features.Sessions.Domain.UseCases;
using Halaqa.Desktop.Features.Sessions.Presentation.Stores;

namespace Halaqa.Desktop.Features.Sessions.Presentation.ViewModels;

public sealed partial class LiveSessionViewModel : ObservableObject
{
    private readonly IPeerMediaConnection _peerMediaConnection;
    private readonly IMushafRealtimeChannel _mushafRealtimeChannel;
    private readonly ILocalVideoRecorder _localVideoRecorder;
    private readonly SaveOfficialMushafStateUseCase _saveOfficialMushafStateUseCase;
    private readonly GetQuranPageUseCase _getQuranPageUseCase;

    [ObservableProperty]
    private Guid _sessionId;

    [ObservableProperty]
    private Guid _taskId;

    [ObservableProperty]
    private string? _operationMessage;

    [ObservableProperty]
    private QuranPage? _quranPage;

    [ObservableProperty]
    private QuranAyah? _selectedAyah;

    [ObservableProperty]
    private string _pageNumberInput = "1";

    [ObservableProperty]
    private bool _isQuranLoading;

    [ObservableProperty]
    private string? _quranMessage;

    public LiveSessionStore Store { get; }

    public string QuranSourceLabel => QuranPage?.IsFromLocalCache == true
        ? "المصحف المحلي — قراءة فقط"
        : "المصحف";

    public string ConnectionLabel => Store.ConnectionState switch
    {
        LiveSessionState.Connected => "اتصال مباشر P2P",
        LiveSessionState.DirectConnectionUnavailable => "الاتصال المباشر غير متاح",
        LiveSessionState.Negotiating or LiveSessionState.Reconnecting => "جارِ التفاوض المباشر",
        _ => "في انتظار تهيئة الجلسة"
    };

    public LiveSessionViewModel(
        LiveSessionStore store,
        IPeerMediaConnection peerMediaConnection,
        IMushafRealtimeChannel mushafRealtimeChannel,
        ILocalVideoRecorder localVideoRecorder,
        SaveOfficialMushafStateUseCase saveOfficialMushafStateUseCase,
        GetQuranPageUseCase getQuranPageUseCase)
    {
        Store = store;
        _peerMediaConnection = peerMediaConnection;
        _mushafRealtimeChannel = mushafRealtimeChannel;
        _localVideoRecorder = localVideoRecorder;
        _saveOfficialMushafStateUseCase = saveOfficialMushafStateUseCase;
        _getQuranPageUseCase = getQuranPageUseCase;

        _peerMediaConnection.StateChanged += (_, state) =>
        {
            Store.SetConnectionState(state.State, state.Reason);
            OnPropertyChanged(nameof(ConnectionLabel));
        };
        _peerMediaConnection.RemoteMediaStateChanged += (_, state) => Store.SetPeerMedia(state.IsMicrophoneMuted, state.IsCameraEnabled);
        _mushafRealtimeChannel.PresenceReceived += (_, state) => Store.SetPeerMushafPresence(state);
        _localVideoRecorder.StateChanged += (_, state) => Store.SetRecording(state);
    }

    [RelayCommand]
    private async Task ToggleMicrophoneAsync()
    {
        var isMuted = !Store.Media.IsMicrophoneMuted;
        await _peerMediaConnection.SetMicrophoneMutedAsync(isMuted);
        Store.SetMicrophoneMuted(isMuted);
    }

    [RelayCommand]
    private async Task ToggleCameraAsync()
    {
        var isEnabled = !Store.Media.IsCameraEnabled;
        await _peerMediaConnection.SetCameraEnabledAsync(isEnabled);
        Store.SetCameraEnabled(isEnabled);
    }

    public async Task InitializeMushafAsync(CancellationToken cancellationToken = default) =>
        await LoadMushafPageAsync(1, cancellationToken: cancellationToken);

    [RelayCommand(CanExecute = nameof(CanLoadQuranPage))]
    private async Task LoadMushafPageAsync() =>
        await LoadMushafPageFromInputAsync();

    [RelayCommand(CanExecute = nameof(CanLoadQuranPage))]
    private async Task PreviousMushafPageAsync()
    {
        var page = QuranPage?.PageNumber ?? ParsePageNumber(PageNumberInput) ?? 1;
        await LoadMushafPageAsync(Math.Max(1, page - 1));
    }

    [RelayCommand(CanExecute = nameof(CanLoadQuranPage))]
    private async Task NextMushafPageAsync()
    {
        var page = QuranPage?.PageNumber ?? ParsePageNumber(PageNumberInput) ?? 1;
        await LoadMushafPageAsync(Math.Min(604, page + 1));
    }

    [RelayCommand(CanExecute = nameof(CanLoadQuranPage))]
    private async Task FollowPeerMushafAsync()
    {
        var peerPage = Store.PeerMushafPresence.PageNumber;
        if (peerPage is null)
        {
            QuranMessage = "لم يشارك الطرف الآخر موضعاً للمصحف بعد.";
            return;
        }

        Store.SetLocalMushafPresence(Store.LocalMushafPresence with { IsFollowingPeer = true });
        await LoadMushafPageAsync(peerPage.Value, Store.PeerMushafPresence.AyahId);
    }

    [RelayCommand]
    private async Task PublishMushafPresenceAsync()
    {
        if (Store.LocalMushafPresence.PageNumber is null)
        {
            OperationMessage = "حمّل صفحة من المصحف أولاً قبل مشاركة الموضع.";
            return;
        }
        if (Store.ConnectionState != LiveSessionState.Connected)
        {
            OperationMessage = "تُرسل مشاركة موضع المصحف بعد اكتمال الاتصال المباشر فقط.";
            return;
        }

        await _mushafRealtimeChannel.SendPresenceAsync(Store.LocalMushafPresence);
        OperationMessage = "أُرسل موضع المصحف مؤقتاً عبر قناة الطرفين.";
    }

    [RelayCommand]
    private async Task RequestRepeatAsync()
    {
        if (SessionId == Guid.Empty || TaskId == Guid.Empty)
        {
            OperationMessage = "لا يمكن إرسال طلب إعادة قبل تهيئة الجلسة والمهمة.";
            return;
        }
        if (Store.ConnectionState != LiveSessionState.Connected)
        {
            OperationMessage = "يتطلب طلب الإعادة اتصالاً مباشراً مكتملاً.";
            return;
        }

        await _mushafRealtimeChannel.SendRepeatRequestAsync(new PeerRepeatRequest(
            SessionId,
            TaskId,
            Store.LocalMushafPresence.AyahId,
            "إعادة التلاوة"));
    }

    [RelayCommand]
    private async Task SaveOfficialMushafStateAsync()
    {
        var state = Store.LocalMushafPresence;
        if (state.PageNumber is null)
        {
            OperationMessage = "اختر صفحة قبل تثبيت موضع المصحف.";
            return;
        }

        var result = await _saveOfficialMushafStateUseCase.ExecuteAsync(
            SessionId,
            state.EditionId,
            state.PageNumber.Value,
            state.AyahId,
            Guid.NewGuid());
        OperationMessage = result.IsSuccess
            ? "تم تثبيت موضع المصحف رسمياً."
            : result.Error?.Message;
    }

    partial void OnSelectedAyahChanged(QuranAyah? value)
    {
        if (value is null)
        {
            return;
        }

        Store.SetLocalMushafPresence(Store.LocalMushafPresence with
        {
            EditionId = value.EditionId,
            PageNumber = value.PageNumber,
            AyahId = value.Id,
            WordIndex = null,
            IsFollowingPeer = false
        });
        QuranMessage = "تم اختيار الآية محلياً. شارك الموضع لإرساله فورياً للطرف الآخر.";
    }

    partial void OnPageNumberInputChanged(string value) => LoadMushafPageCommand.NotifyCanExecuteChanged();

    private bool CanLoadQuranPage() => !IsQuranLoading;

    private async Task LoadMushafPageFromInputAsync()
    {
        var pageNumber = ParsePageNumber(PageNumberInput);
        if (pageNumber is null)
        {
            QuranMessage = "أدخل رقم صفحة بين 1 و604.";
            return;
        }

        await LoadMushafPageAsync(pageNumber.Value);
    }

    private async Task LoadMushafPageAsync(
        int pageNumber,
        int? selectedAyahId = null,
        CancellationToken cancellationToken = default)
    {
        IsQuranLoading = true;
        QuranMessage = null;
        try
        {
            var result = await _getQuranPageUseCase.ExecuteAsync(1, pageNumber, cancellationToken);
            if (!result.IsSuccess || result.Value is null)
            {
                QuranMessage = result.Error?.Message ?? "تعذر تحميل صفحة المصحف.";
                return;
            }

            QuranPage = result.Value;
            PageNumberInput = result.Value.PageNumber.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var selected = selectedAyahId is { } ayahId
                ? result.Value.Ayahs.FirstOrDefault(ayah => ayah.Id == ayahId)
                : null;
            SelectedAyah = selected;
            Store.SetLocalMushafPresence(Store.LocalMushafPresence with
            {
                EditionId = result.Value.EditionId,
                PageNumber = result.Value.PageNumber,
                AyahId = selected?.Id,
                WordIndex = null
            });
            QuranMessage = result.Value.IsFromLocalCache
                ? "حُمّلت الصفحة من قاعدة المصحف المحلية للقراءة فقط."
                : "حُمّلت الصفحة من المصدر المتاح.";
            OnPropertyChanged(nameof(QuranSourceLabel));
        }
        finally
        {
            IsQuranLoading = false;
            LoadMushafPageCommand.NotifyCanExecuteChanged();
            PreviousMushafPageCommand.NotifyCanExecuteChanged();
            NextMushafPageCommand.NotifyCanExecuteChanged();
            FollowPeerMushafCommand.NotifyCanExecuteChanged();
        }
    }

    private static int? ParsePageNumber(string? value) =>
        int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var pageNumber) &&
        pageNumber is >= 1 and <= 604
            ? pageNumber
            : null;

    [RelayCommand]
    private async Task ToggleLocalRecordingAsync()
    {
        if (Store.Recording.State == RecordingState.Recording)
        {
            await _localVideoRecorder.StopAsync();
            return;
        }

        var outputDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
            "Halaqa");
        await _localVideoRecorder.StartAsync(outputDirectory);
    }
}
