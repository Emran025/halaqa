namespace Halaqa.Desktop.Features.Sessions.Domain.Entities;

public enum LiveSessionState
{
    Requested,
    Accepted,
    Preparing,
    Negotiating,
    Connected,
    Reconnecting,
    DirectConnectionUnavailable,
    Ending,
    Ended
}

public enum RecordingState
{
    Idle,
    Starting,
    Recording,
    Stopping,
    Failed
}

public sealed record RealtimeSessionConfig(
    Guid SessionId,
    string ChannelName,
    Uri WebSocketUrl,
    DateTimeOffset ExpiresAt,
    bool DirectP2POnly,
    string SignalingTransport,
    string IceCandidatePolicy,
    string? MediaTransport);

public sealed record MediaState(
    bool IsMicrophoneMuted,
    bool IsCameraEnabled,
    bool IsRemoteMicrophoneMuted,
    bool IsRemoteCameraEnabled,
    string? SelectedMicrophoneId,
    string? SelectedCameraId);

public sealed record MushafPresenceState(
    int EditionId,
    int? PageNumber,
    int? AyahId,
    int? WordIndex,
    bool IsFollowingPeer);

public sealed record LocalRecordingState(
    RecordingState State,
    string? OutputPath,
    TimeSpan Duration,
    string? ErrorMessage);

public sealed record LiveSessionSnapshot(
    Guid SessionId,
    Guid PeerId,
    LiveSessionState State,
    MediaState Media,
    MushafPresenceState MushafPresence,
    LocalRecordingState Recording);
