using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    [ObservableProperty]
    private Guid _sessionId;

    [ObservableProperty]
    private Guid _taskId;

    [ObservableProperty]
    private string? _operationMessage;

    public LiveSessionStore Store { get; }

    public LiveSessionViewModel(
        LiveSessionStore store,
        IPeerMediaConnection peerMediaConnection,
        IMushafRealtimeChannel mushafRealtimeChannel,
        ILocalVideoRecorder localVideoRecorder,
        SaveOfficialMushafStateUseCase saveOfficialMushafStateUseCase)
    {
        Store = store;
        _peerMediaConnection = peerMediaConnection;
        _mushafRealtimeChannel = mushafRealtimeChannel;
        _localVideoRecorder = localVideoRecorder;
        _saveOfficialMushafStateUseCase = saveOfficialMushafStateUseCase;

        _peerMediaConnection.StateChanged += (_, state) => Store.SetConnectionState(state.State, state.Reason);
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

    [RelayCommand]
    private async Task PublishMushafPresenceAsync()
    {
        await _mushafRealtimeChannel.SendPresenceAsync(Store.LocalMushafPresence);
    }

    [RelayCommand]
    private async Task RequestRepeatAsync()
    {
        if (SessionId == Guid.Empty || TaskId == Guid.Empty)
        {
            OperationMessage = "لا يمكن إرسال طلب إعادة قبل تهيئة الجلسة والمهمة.";
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
