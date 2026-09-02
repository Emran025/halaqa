using Halaqa.Desktop.Features.Notes.Domain.Entities;
using Halaqa.Desktop.Features.Notes.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Notes.Domain.UseCases;

public sealed class ListTaskNotesUseCase
{
    private readonly ITaskNoteRepository repository;

    public ListTaskNotesUseCase(ITaskNoteRepository repository)
    {
        this.repository = repository;
    }

    public Task<Result<TaskNotePage>> ExecuteAsync(Guid sessionId, Guid taskId, CancellationToken cancellationToken = default) =>
        HasValidRoute(sessionId, taskId)
            ? repository.ListAsync(sessionId, taskId, cancellationToken)
            : Task.FromResult(Result<TaskNotePage>.Failure(InvalidRoute()));

    private static bool HasValidRoute(Guid sessionId, Guid taskId) => sessionId != Guid.Empty && taskId != Guid.Empty;

    private static AppError InvalidRoute() => new(AppErrorKind.Validation, "معرّف الجلسة أو المهمة غير صالح.");
}

public sealed class CreateTaskNoteUseCase
{
    private readonly ITaskNoteRepository repository;

    public CreateTaskNoteUseCase(ITaskNoteRepository repository)
    {
        this.repository = repository;
    }

    public Task<Result<TaskNote>> ExecuteAsync(CreateTaskNoteCommand command, CancellationToken cancellationToken = default)
    {
        var error = TaskNoteValidation.Validate(command.SessionId, command.TaskId, command.Body, command.ClientOperationId);
        return error is null
            ? repository.CreateAsync(command, cancellationToken)
            : Task.FromResult(Result<TaskNote>.Failure(error));
    }
}

public sealed class UpdateTaskNoteUseCase
{
    private readonly ITaskNoteRepository repository;

    public UpdateTaskNoteUseCase(ITaskNoteRepository repository)
    {
        this.repository = repository;
    }

    public Task<Result<TaskNote>> ExecuteAsync(UpdateTaskNoteCommand command, CancellationToken cancellationToken = default)
    {
        var error = TaskNoteValidation.Validate(command.SessionId, command.TaskId, command.NoteId, command.Body);
        return error is null
            ? repository.UpdateAsync(command, cancellationToken)
            : Task.FromResult(Result<TaskNote>.Failure(error));
    }
}

public sealed class DeleteTaskNoteUseCase
{
    private readonly ITaskNoteRepository repository;

    public DeleteTaskNoteUseCase(ITaskNoteRepository repository)
    {
        this.repository = repository;
    }

    public Task<Result> ExecuteAsync(DeleteTaskNoteCommand command, CancellationToken cancellationToken = default) =>
        command.SessionId != Guid.Empty && command.TaskId != Guid.Empty && command.NoteId != Guid.Empty
            ? repository.DeleteAsync(command, cancellationToken)
            : Task.FromResult(Result.Failure(new AppError(AppErrorKind.Validation, "معرّف الجلسة أو المهمة أو الملاحظة غير صالح.")));
}

internal static class TaskNoteValidation
{
    public static AppError? Validate(Guid sessionId, Guid taskId, string? body, Guid clientOperationId)
    {
        if (sessionId == Guid.Empty || taskId == Guid.Empty || clientOperationId == Guid.Empty)
        {
            return new AppError(AppErrorKind.Validation, "تعذر تجهيز طلب الملاحظة.");
        }
        return ValidateBody(body);
    }

    public static AppError? Validate(Guid sessionId, Guid taskId, Guid noteId, string? body)
    {
        if (sessionId == Guid.Empty || taskId == Guid.Empty || noteId == Guid.Empty)
        {
            return new AppError(AppErrorKind.Validation, "معرّف الجلسة أو المهمة أو الملاحظة غير صالح.");
        }
        return ValidateBody(body);
    }

    private static AppError? ValidateBody(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return new AppError(AppErrorKind.Validation, "نص الملاحظة مطلوب.");
        }
        if (body.Trim().Length > 2000)
        {
            return new AppError(AppErrorKind.Validation, "نص الملاحظة يجب ألا يتجاوز 2000 حرف.");
        }
        return null;
    }
}
