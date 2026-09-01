using Halaqa.Desktop.Features.Quran.Data.Models;
using Halaqa.Desktop.Features.Quran.Domain.Entities;

namespace Halaqa.Desktop.Features.Quran.Data.Mappers;

internal static class QuranMapper
{
    public static QuranPage ToDomain(QuranPageDto page, bool isFromLocalCache) =>
        new(
            page.EditionId,
            page.PageNumber,
            (page.Surahs ?? Array.Empty<QuranSurahDto>()).Select(surah => new QuranSurah(
                surah.Id,
                surah.EditionId,
                surah.Number,
                surah.Name,
                surah.AyahCount,
                surah.RevelationPlace)).ToArray(),
            (page.Ayahs ?? Array.Empty<QuranAyahDto>()).Select(ayah => new QuranAyah(
                ayah.Id,
                ayah.EditionId,
                ayah.SurahId,
                ayah.ResolvedNumber,
                ayah.PageNumber,
                ayah.ResolvedText,
                ayah.ResolvedText,
                ayah.ResolvedJuz,
                (ayah.Words ?? Array.Empty<QuranWordDto>()).Select(word => new QuranWord(word.Index, word.Text)).ToArray())).ToArray(),
            isFromLocalCache);
}
