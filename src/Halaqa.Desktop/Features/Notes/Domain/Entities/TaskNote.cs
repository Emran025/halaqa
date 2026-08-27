namespace Halaqa.Desktop.Features.Notes.Domain.Entities;

public sealed record TaskNoteAuthor(Guid Id, string Name);

public sealed record TaskNote(
    Guid Id,
    string Body,
    TaskNoteAuthor Author,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record TaskNotePage(IReadOnlyList<TaskNote> Notes);

public sealed record CreateTaskNoteCommand(Guid SessionId, Guid TaskId, string Body, Guid ClientOperationId);

public sealed record UpdateTaskNoteCommand(Guid SessionId, Guid TaskId, Guid NoteId, string Body);

public sealed record DeleteTaskNoteCommand(Guid SessionId, Guid TaskId, Guid NoteId);
