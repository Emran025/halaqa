namespace Halaqa.Desktop.Features.Quran.Domain.Entities;

public sealed record QuranWord(int Index, string Text);

public sealed record QuranAyah(
    int Id,
    int EditionId,
    int SurahId,
    int Number,
    int PageNumber,
    string Text,
    string PageGlyphText,
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
    bool IsFromLocalCache)
{
    public string PageGlyphText => IsFromLocalCache
        ? string.Concat(Ayahs.Select(ayah => ayah.PageGlyphText))
        : string.Join(" ", Ayahs.Select(ayah => $"{ayah.Text} ﴿{ayah.Number}﴾"));
}

public sealed record QuranSurahIndexItem(
    int Number,
    string Name,
    int AyahCount,
    int StartPage,
    string? RevelationPlace);

public sealed record QuranJuzIndexItem(
    int Number,
    string Name,
    int StartPage,
    int EndPage);
