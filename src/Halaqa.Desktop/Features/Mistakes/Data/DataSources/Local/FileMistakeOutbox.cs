using System.Text.Json;
using Halaqa.Desktop.Features.Mistakes.Domain.Entities;

namespace Halaqa.Desktop.Features.Mistakes.Data.DataSources.Local;

internal interface IMistakeOutbox
{
    Task<IReadOnlyList<PendingMistakeOperation>> ReadAsync(CancellationToken cancellationToken = default);
    Task UpsertAsync(PendingMistakeOperation operation, CancellationToken cancellationToken = default);
}

internal sealed class FileMistakeOutbox : IMistakeOutbox
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
    private readonly string _outboxPath;

    public FileMistakeOutbox()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Halaqa",
            "mistakes");
        Directory.CreateDirectory(directory);
        _outboxPath = Path.Combine(directory, "outbox.json");
    }

    public async Task<IReadOnlyList<PendingMistakeOperation>> ReadAsync(CancellationToken cancellationToken = default)
    {
        await Gate.WaitAsync(cancellationToken);
        try
        {
            return await ReadCoreAsync(cancellationToken);
        }
        finally
        {
            Gate.Release();
        }
    }

    public async Task UpsertAsync(PendingMistakeOperation operation, CancellationToken cancellationToken = default)
    {
        await Gate.WaitAsync(cancellationToken);
        try
        {
            var operations = (await ReadCoreAsync(cancellationToken)).ToList();
            var index = operations.FindIndex(item => item.LocalId == operation.LocalId);
            if (index >= 0)
            {
                operations[index] = operation;
            }
            else
            {
                operations.Add(operation);
            }

            var temporaryPath = $"{_outboxPath}.tmp";
            await using (var stream = File.Create(temporaryPath))
            {
                await JsonSerializer.SerializeAsync(stream, operations, JsonOptions, cancellationToken);
            }

            File.Move(temporaryPath, _outboxPath, overwrite: true);
        }
        finally
        {
            Gate.Release();
        }
    }

    private async Task<IReadOnlyList<PendingMistakeOperation>> ReadCoreAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_outboxPath))
        {
            return Array.Empty<PendingMistakeOperation>();
        }

        try
        {
            await using var stream = File.OpenRead(_outboxPath);
            return await JsonSerializer.DeserializeAsync<List<PendingMistakeOperation>>(stream, JsonOptions, cancellationToken)
                ?? Array.Empty<PendingMistakeOperation>();
        }
        catch (JsonException)
        {
            return Array.Empty<PendingMistakeOperation>();
        }
    }
}
