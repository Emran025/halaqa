using System.Text.Json.Serialization;

namespace Halaqa.Desktop.Features.Notes.Data.Models;

internal sealed record CreateTaskNoteRequestDto(
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("client_operation_id")] Guid ClientOperationId);

internal sealed record UpdateTaskNoteRequestDto(
    [property: JsonPropertyName("body")] string Body);

internal sealed record TaskNoteAuthorDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name);

internal sealed record TaskNoteDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("author")] TaskNoteAuthorDto Author,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset? UpdatedAt);

internal sealed record TaskNoteResponseDto([property: JsonPropertyName("note")] TaskNoteDto Note);

internal sealed record TaskNoteCollectionResponseDto([property: JsonPropertyName("notes")] IReadOnlyList<TaskNoteDto> Notes);
