using System.Text.Json.Serialization;

namespace Halaqa.Desktop.Features.Quran.Data.Models;

internal sealed record QuranPageResponseDto(
    [property: JsonPropertyName("quran_page")] QuranPageDto QuranPage);

internal sealed record QuranPageDto(
    [property: JsonPropertyName("edition_id")] int EditionId,
    [property: JsonPropertyName("page_number")] int PageNumber,
    [property: JsonPropertyName("surahs")] IReadOnlyList<QuranSurahDto> Surahs,
    [property: JsonPropertyName("ayahs")] IReadOnlyList<QuranAyahDto> Ayahs);

internal sealed record QuranSurahDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("edition_id")] int EditionId,
    [property: JsonPropertyName("number")] int Number,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("ayah_count")] int AyahCount,
    [property: JsonPropertyName("revelation_place")] string? RevelationPlace);

internal sealed record QuranAyahDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("edition_id")] int EditionId,
    [property: JsonPropertyName("surah_id")] int SurahId,
    [property: JsonPropertyName("number")] int Number,
    [property: JsonPropertyName("page_number")] int PageNumber,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("juz")] int? Juz,
    [property: JsonPropertyName("words")] IReadOnlyList<QuranWordDto> Words);

internal sealed record QuranWordDto(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("text")] string Text);
