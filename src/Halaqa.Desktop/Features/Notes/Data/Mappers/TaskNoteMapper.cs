using Halaqa.Desktop.Features.Notes.Data.Models;
using Halaqa.Desktop.Features.Notes.Domain.Entities;

namespace Halaqa.Desktop.Features.Notes.Data.Mappers;

internal static class TaskNoteMapper
{
    public static TaskNotePage ToDomain(TaskNoteCollectionResponseDto dto) =>
        new(dto.Notes.Select(ToDomain).ToArray());

    public static TaskNote ToDomain(TaskNoteDto dto) =>
        new(dto.Id, dto.Body, new TaskNoteAuthor(dto.Author.Id, dto.Author.Name), dto.CreatedAt, dto.UpdatedAt);
}
