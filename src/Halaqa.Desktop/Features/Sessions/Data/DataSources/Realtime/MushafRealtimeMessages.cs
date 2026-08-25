using System.Text.Json.Serialization;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;

namespace Halaqa.Desktop.Features.Sessions.Data.DataSources.Realtime;

internal sealed record MushafPresenceMessageDto(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("edition_id")] int EditionId,
    [property: JsonPropertyName("page_number")] int PageNumber,
    [property: JsonPropertyName("ayah_id")] int? AyahId,
    [property: JsonPropertyName("word_index")] int? WordIndex);

internal sealed record RepeatRequestMessageDto(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("session_id")] Guid SessionId,
    [property: JsonPropertyName("task_id")] Guid TaskId,
    [property: JsonPropertyName("ayah_id")] int? AyahId,
    [property: JsonPropertyName("reason")] string? Reason);

internal static class MushafRealtimeMessageFactory
{
    public static MushafPresenceMessageDto CreatePresence(MushafPresenceState state)
    {
        if (state.PageNumber is null)
        {
            throw new InvalidOperationException("لا يمكن بث حضور مصحف بلا رقم صفحة.");
        }

        return new MushafPresenceMessageDto(
            state.AyahId is null ? "mushaf.page_changed" : "mushaf.ayah_selected",
            state.EditionId,
            state.PageNumber.Value,
            state.AyahId,
            state.WordIndex);
    }

    public static RepeatRequestMessageDto CreateRepeatRequest(PeerRepeatRequest request) =>
        new("guidance.request_repeat", request.SessionId, request.TaskId, request.AyahId, request.Reason);
}
