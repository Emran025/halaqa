namespace Halaqa.Desktop.Features.Quran.Domain.Entities;

public sealed record QuranWord(int Index, string Text);

public sealed record QuranAyah(
    int Id,
    int EditionId,
    int SurahId,
    int Number,
    int PageNumber,
    string Text,
    int? Juz,
    IReadOnlyList<QuranWord> Words);

public sealed record QuranSurah(
    int Id,
    int EditionId,
    int Number,
    string Name,
    int AyahCount,
    string? RevelationPlace);

public sealed record QuranPage(
    int EditionId,
    int PageNumber,
    IReadOnlyList<QuranSurah> Surahs,
    IReadOnlyList<QuranAyah> Ayahs,
    bool IsFromLocalCache);
