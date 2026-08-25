using Halaqa.Desktop.Features.Quran.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;
using Microsoft.Data.Sqlite;

namespace Halaqa.Desktop.Features.Quran.Data.DataSources.Local;

internal interface IQuranLocalDataSource
{
    Task<Result<QuranPage>> GetPageAsync(int editionId, int pageNumber, CancellationToken cancellationToken = default);
}

internal sealed class SqliteQuranLocalDataSource : IQuranLocalDataSource
{
    private const string DatabaseName = "QuranV3.sqlite";
    private readonly SemaphoreSlim _copyGate = new(1, 1);
    private string? _databasePath;

    public async Task<Result<QuranPage>> GetPageAsync(
        int editionId,
        int pageNumber,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var databasePath = await EnsureDatabaseAsync(cancellationToken);
            await using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = databasePath,
                Mode = SqliteOpenMode.ReadOnly,
                ForeignKeys = true
            }.ToString());
            await connection.OpenAsync(cancellationToken);

            var surahs = await GetSurahsAsync(connection, editionId, pageNumber, cancellationToken);
            var ayahs = await GetAyahsAsync(connection, editionId, pageNumber, cancellationToken);
            return Result<QuranPage>.Success(new QuranPage(editionId, pageNumber, surahs, ayahs, IsFromLocalCache: true));
        }
        catch (SqliteException exception)
        {
            return Result<QuranPage>.Failure(new AppError(AppErrorKind.Cache, "تعذر فتح قاعدة المصحف المحلية.", Code: exception.SqliteErrorCode.ToString()));
        }
        catch (IOException)
        {
            return Result<QuranPage>.Failure(new AppError(AppErrorKind.Cache, "تعذر تهيئة نسخة المصحف المحلية."));
        }
    }

    private async Task<string> EnsureDatabaseAsync(CancellationToken cancellationToken)
    {
        var existingPath = _databasePath;
        if (existingPath is not null && File.Exists(existingPath))
        {
            return existingPath;
        }

        await _copyGate.WaitAsync(cancellationToken);
        try
        {
            var dataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Halaqa",
                "Data");
            Directory.CreateDirectory(dataDirectory);

            var targetPath = Path.Combine(dataDirectory, DatabaseName);
            if (!File.Exists(targetPath))
            {
                var sourcePath = Path.Combine(AppContext.BaseDirectory, "Assets", "Quran", DatabaseName);
                if (!File.Exists(sourcePath))
                {
                    throw new IOException("لم يعثر التطبيق على ملف قاعدة المصحف المضمن.");
                }

                var temporaryPath = $"{targetPath}.tmp";
                await using (var source = File.OpenRead(sourcePath))
                await using (var destination = File.Create(temporaryPath))
                {
                    await source.CopyToAsync(destination, cancellationToken);
                }

                File.Move(temporaryPath, targetPath, overwrite: true);
            }

            _databasePath = targetPath;
            return targetPath;
        }
        finally
        {
            _copyGate.Release();
        }
    }

    private static async Task<IReadOnlyList<QuranSurah>> GetSurahsAsync(
        SqliteConnection connection,
        int editionId,
        int pageNumber,
        CancellationToken cancellationToken)
    {
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT DISTINCT s.Id, s.Name_ar, s.AyatCount, s.TypeText_ar
            FROM Quran q
            INNER JOIN Sora s ON s.Id = q.SoraNum
            WHERE q.PageNum = $pageNumber
            ORDER BY s.Id;
            """;
        command.Parameters.AddWithValue("$pageNumber", pageNumber);

        var result = new List<QuranSurah>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var id = reader.GetInt32(0);
            result.Add(new QuranSurah(
                id,
                editionId,
                id,
                reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                reader.IsDBNull(3) ? null : reader.GetString(3)));
        }

        return result;
    }

    private static async Task<IReadOnlyList<QuranAyah>> GetAyahsAsync(
        SqliteConnection connection,
        int editionId,
        int pageNumber,
        CancellationToken cancellationToken)
    {
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT ID, SoraNum, AyaNum, PageNum, AyaDiac, Uthomanic_text, PartNum
            FROM Quran
            WHERE PageNum = $pageNumber
            ORDER BY ID;
            """;
        command.Parameters.AddWithValue("$pageNumber", pageNumber);

        var result = new List<QuranAyah>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var ayahId = reader.GetInt32(0);
            var text = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
            var pageGlyphText = reader.IsDBNull(5)
                ? string.Empty
                : DecodePageGlyphText(reader.GetString(5));
            result.Add(new QuranAyah(
                ayahId,
                editionId,
                reader.GetInt32(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                text,
                pageGlyphText,
                reader.IsDBNull(6) ? null : reader.GetInt32(6),
                ToWords(pageGlyphText)));
        }

        return result;
    }

    private static string DecodePageGlyphText(string encodedGlyphText) =>
        encodedGlyphText.Replace("\\n", "\n", StringComparison.Ordinal);

    private static IReadOnlyList<QuranWord> ToWords(string pageGlyphText) =>
        pageGlyphText
            .Select((glyph, index) => new QuranWord(index, glyph.ToString()))
            .ToArray();
}
