using CommunityToolkit.Mvvm.ComponentModel;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;

namespace Halaqa.Desktop.Features.Sessions.Presentation.Stores;

public sealed partial class LiveSessionStore : ObservableObject
{
    [ObservableProperty]
    private LiveSessionState _connectionState = LiveSessionState.Requested;

    [ObservableProperty]
    private MediaState _media = new(
        IsMicrophoneMuted: false,
        IsCameraEnabled: true,
        IsRemoteMicrophoneMuted: false,
        IsRemoteCameraEnabled: true,
        SelectedMicrophoneId: null,
        SelectedCameraId: null);

    [ObservableProperty]
    private MushafPresenceState _localMushafPresence = new(
        EditionId: 1,
        PageNumber: null,
        AyahId: null,
        WordIndex: null,
        IsFollowingPeer: false);

    [ObservableProperty]
    private MushafPresenceState _peerMushafPresence = new(
        EditionId: 1,
        PageNumber: null,
        AyahId: null,
        WordIndex: null,
        IsFollowingPeer: false);

    [ObservableProperty]
    private LocalRecordingState _recording = new(
        RecordingState.Idle,
        OutputPath: null,
        Duration: TimeSpan.Zero,
        ErrorMessage: null);

    [ObservableProperty]
    private string? _connectionMessage;

    public void SetConnectionState(LiveSessionState state, string? message = null)
    {
        ConnectionState = state;
        ConnectionMessage = message;
    }

    public void SetMicrophoneMuted(bool isMuted) =>
        Media = Media with { IsMicrophoneMuted = isMuted };

    public void SetCameraEnabled(bool isEnabled) =>
        Media = Media with { IsCameraEnabled = isEnabled };

    public void SetPeerMedia(bool isMicrophoneMuted, bool isCameraEnabled) =>
        Media = Media with
        {
            IsRemoteMicrophoneMuted = isMicrophoneMuted,
            IsRemoteCameraEnabled = isCameraEnabled
        };

    public void SetLocalMushafPresence(MushafPresenceState state) => LocalMushafPresence = state;
    public void SetPeerMushafPresence(MushafPresenceState state) => PeerMushafPresence = state;
    public void SetRecording(LocalRecordingState state) => Recording = state;
}
