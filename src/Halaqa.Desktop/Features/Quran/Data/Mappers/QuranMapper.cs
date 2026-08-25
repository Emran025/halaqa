using Halaqa.Desktop.Features.Quran.Data.Models;
using Halaqa.Desktop.Features.Quran.Domain.Entities;

namespace Halaqa.Desktop.Features.Quran.Data.Mappers;

internal static class QuranMapper
{
    public static QuranPage ToDomain(QuranPageDto page, bool isFromLocalCache) =>
        new(
            page.EditionId,
            page.PageNumber,
            page.Surahs.Select(surah => new QuranSurah(
                surah.Id,
                surah.EditionId,
                surah.Number,
                surah.Name,
                surah.AyahCount,
                surah.RevelationPlace)).ToArray(),
            page.Ayahs.Select(ayah => new QuranAyah(
                ayah.Id,
                ayah.EditionId,
                ayah.SurahId,
                ayah.Number,
                ayah.PageNumber,
                ayah.Text,
                ayah.Text,
                ayah.Juz,
                ayah.Words.Select(word => new QuranWord(word.Index, word.Text)).ToArray())).ToArray(),
            isFromLocalCache);
}
