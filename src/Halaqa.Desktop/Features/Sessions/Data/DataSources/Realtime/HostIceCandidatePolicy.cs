using Halaqa.Desktop.Features.Sessions.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Data.DataSources.Realtime;

internal static class HostIceCandidatePolicy
{
    public static Result<HostIceCandidate> Validate(
        string candidate,
        string? sdpMid,
        int sdpMLineIndex,
        string? usernameFragment)
    {
        if (string.IsNullOrWhiteSpace(candidate) || sdpMLineIndex < 0)
        {
            return Result<HostIceCandidate>.Failure(new AppError(
                AppErrorKind.Validation,
                "مرشح ICE غير صالح."));
        }

        var normalized = candidate.Trim().ToLowerInvariant();
        var isHostCandidate = normalized.Contains(" typ host", StringComparison.Ordinal);
        var hasForbiddenType = normalized.Contains(" typ srflx", StringComparison.Ordinal) ||
                               normalized.Contains(" typ prflx", StringComparison.Ordinal) ||
                               normalized.Contains(" typ relay", StringComparison.Ordinal);
        if (!isHostCandidate || hasForbiddenType)
        {
            return Result<HostIceCandidate>.Failure(new AppError(
                AppErrorKind.Forbidden,
                "سياسة الجلسة تسمح بمرشحات Host ICE المباشرة فقط."));
        }

        return Result<HostIceCandidate>.Success(new HostIceCandidate(
            candidate,
            sdpMid,
            sdpMLineIndex,
            usernameFragment));
    }
}
