using System.Text.Json;
using Halaqa.Desktop.Config.Persistence;
using Halaqa.Desktop.Features.Mistakes.Domain.Entities;
using Microsoft.Data.Sqlite;

namespace Halaqa.Desktop.Features.Mistakes.Data.DataSources.Local;

internal interface IMistakeOutbox
{
    Task<IReadOnlyList<PendingMistakeOperation>> ReadAsync(CancellationToken cancellationToken = default);
    Task UpsertAsync(PendingMistakeOperation operation, CancellationToken cancellationToken = default);
}

internal sealed class SqliteMistakeOutbox : IMistakeOutbox
{

    private readonly ILocalDatabase localDatabase;


    public SqliteMistakeOutbox(

        ILocalDatabase localDatabase

    )

    {

        this.localDatabase = localDatabase;

    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<PendingMistakeOperation>> ReadAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await localDatabase.OpenConnectionAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT local_id, payload_json, sync_state, created_at_utc, last_error
            FROM mistake_outbox
            ORDER BY created_at_utc ASC;
            ";

        var operations = new List<PendingMistakeOperation>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var localId = Guid.Parse(reader.GetString(0));
            var draft = JsonSerializer.Deserialize<MistakeDraft>(reader.GetString(1), JsonOptions);
            if (draft is null || !Enum.TryParse<MistakeSyncState>(reader.GetString(2), ignoreCase: true, out var syncState))
            {
                continue;
            }

            var createdAt = DateTimeOffset.Parse(reader.GetString(3), null, System.Globalization.DateTimeStyles.RoundtripKind);
            operations.Add(new PendingMistakeOperation(
                localId,
                draft,
                syncState,
                createdAt,
                reader.IsDBNull(4) ? null : reader.GetString(4)));
        }

        return operations;
    }

    public async Task UpsertAsync(PendingMistakeOperation operation, CancellationToken cancellationToken = default)
    {
        await using var connection = await localDatabase.OpenConnectionAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO mistake_outbox (
                local_id, client_operation_id, session_id, task_id, operation_type,
                payload_json, sync_state, created_at_utc, last_error)
            VALUES (
                $localId, $clientOperationId, $sessionId, $taskId, 'create',
                $payloadJson, $syncState, $createdAtUtc, $lastError)
            ON CONFLICT(local_id) DO UPDATE SET
                payload_json = excluded.payload_json,
                sync_state = excluded.sync_state,
                last_error = excluded.last_error;
            ";
        command.Parameters.AddWithValue("$localId", operation.LocalId.ToString());
        command.Parameters.AddWithValue("$clientOperationId", operation.Draft.ClientOperationId.ToString());
        command.Parameters.AddWithValue("$sessionId", operation.Draft.SessionId.ToString());
        command.Parameters.AddWithValue("$taskId", operation.Draft.TaskId.ToString());
        command.Parameters.AddWithValue("$payloadJson", JsonSerializer.Serialize(operation.Draft, JsonOptions));
        command.Parameters.AddWithValue("$syncState", operation.SyncState.ToString());
        command.Parameters.AddWithValue("$createdAtUtc", operation.CreatedAt.UtcDateTime.ToString("O"));
        command.Parameters.AddWithValue("$lastError", (object?)operation.FailureReason ?? DBNull.Value);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
