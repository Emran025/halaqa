using Microsoft.Data.Sqlite;

namespace Halaqa.Desktop.Config.Persistence;

public interface ILocalDatabase
{
    Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}

public sealed class SqliteLocalDatabase : ILocalDatabase
{
    private readonly SemaphoreSlim _initializationGate = new(1, 1);
    private bool _isInitialized;
    private readonly string _databasePath;

    public SqliteLocalDatabase()
    {
        var dataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Halaqa",
            "Data");
        Directory.CreateDirectory(dataDirectory);
        _databasePath = Path.Combine(dataDirectory, "halaqa-local.db");
    }

    public async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true
        }.ToString());
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_isInitialized)
        {
            return;
        }

        await _initializationGate.WaitAsync(cancellationToken);
        try
        {
            if (_isInitialized)
            {
                return;
            }

            await using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = _databasePath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                ForeignKeys = true
            }.ToString());
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS mistake_outbox (
                    local_id TEXT PRIMARY KEY NOT NULL,
                    client_operation_id TEXT NOT NULL UNIQUE,
                    session_id TEXT NOT NULL,
                    task_id TEXT NOT NULL,
                    operation_type TEXT NOT NULL,
                    payload_json TEXT NOT NULL,
                    sync_state TEXT NOT NULL,
                    created_at_utc TEXT NOT NULL,
                    last_error TEXT NULL
                );
                CREATE INDEX IF NOT EXISTS idx_mistake_outbox_sync_state_created
                    ON mistake_outbox(sync_state, created_at_utc);
                CREATE TABLE IF NOT EXISTS sync_metadata (
                    entity_type TEXT PRIMARY KEY NOT NULL,
                    last_server_sync_utc TEXT NULL
                );
                ";
            await command.ExecuteNonQueryAsync(cancellationToken);
            _isInitialized = true;
        }
        finally
        {
            _initializationGate.Release();
        }
    }
}
