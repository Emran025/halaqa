using System.Text.Json.Serialization;
using Halaqa.Desktop.Config.Http;
using Halaqa.Desktop.Features.Mistakes.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Mistakes.Data.DataSources.Remote;

internal sealed record CreateMistakeRequestDto(
    [property: JsonPropertyName("ayah_id")] int AyahId,
    [property: JsonPropertyName("page_number")] int? PageNumber,
    [property: JsonPropertyName("word_index")] int WordIndex,
    [property: JsonPropertyName("mistake_type")] string MistakeType,
    [property: JsonPropertyName("note")] string? Note,
    [property: JsonPropertyName("client_operation_id")] Guid ClientOperationId);

internal interface IMistakeRemoteDataSource
{
    Task<Result> CreateAsync(MistakeDraft draft, CancellationToken cancellationToken = default);
}

internal sealed class MistakeRemoteDataSource : IMistakeRemoteDataSource
{

    private readonly IApiClient apiClient;


    public MistakeRemoteDataSource(

        IApiClient apiClient

    )

    {

        this.apiClient = apiClient;

    }

    public Task<Result> CreateAsync(MistakeDraft draft, CancellationToken cancellationToken = default)
    {
        var request = new CreateMistakeRequestDto(
            draft.AyahId,
            draft.PageNumber,
            draft.WordIndex,
            draft.MistakeType.ToString().ToLowerInvariant(),
            draft.Note,
            draft.ClientOperationId);
        return apiClient.PostAsync(
            $"sessions/{draft.SessionId}/tasks/{draft.TaskId}/mistakes",
            request,
            cancellationToken);
    }
}
