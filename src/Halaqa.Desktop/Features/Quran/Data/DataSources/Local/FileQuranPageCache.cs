using System.Text.Json;
using Halaqa.Desktop.Features.Quran.Data.Models;

namespace Halaqa.Desktop.Features.Quran.Data.DataSources.Local;

internal interface IQuranPageCache
{
    Task<QuranPageDto?> ReadAsync(int editionId, int pageNumber, CancellationToken cancellationToken = default);
    Task SaveAsync(QuranPageDto page, CancellationToken cancellationToken = default);
}

internal sealed class FileQuranPageCache : IQuranPageCache
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly string _cacheDirectory;

    public FileQuranPageCache()
    {
        _cacheDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Halaqa",
            "quran-cache");
        Directory.CreateDirectory(_cacheDirectory);
    }

    public async Task<QuranPageDto?> ReadAsync(int editionId, int pageNumber, CancellationToken cancellationToken = default)
    {
        var path = GetPath(editionId, pageNumber);
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<QuranPageDto>(stream, JsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
            File.Delete(path);
            return null;
        }
    }

    public async Task SaveAsync(QuranPageDto page, CancellationToken cancellationToken = default)
    {
        var path = GetPath(page.EditionId, page.PageNumber);
        var temporaryPath = $"{path}.tmp";
        await using (var stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(stream, page, JsonOptions, cancellationToken);
        }

        File.Move(temporaryPath, path, overwrite: true);
    }

    private string GetPath(int editionId, int pageNumber) =>
        Path.Combine(_cacheDirectory, $"edition-{editionId}-page-{pageNumber}.json");
}
