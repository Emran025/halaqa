using System.Text.Json.Serialization;

namespace Halaqa.Desktop.Shared.Data.Models;

public sealed class CountryItem
{
    [JsonPropertyName("Country_EN")]
    public string NameEn { get; set; } = string.Empty;

    [JsonPropertyName("Country_AR")]
    public string NameAr { get; set; } = string.Empty;

    [JsonPropertyName("Phone_Code")]
    public string PhoneCode { get; set; } = string.Empty;

    [JsonPropertyName("Match_Name")]
    public string MatchName { get; set; } = string.Empty;

    [JsonPropertyName("alpha2")]
    public string Alpha2 { get; set; } = string.Empty;

    [JsonPropertyName("alpha3")]
    public string Alpha3 { get; set; } = string.Empty;

    [JsonPropertyName("flag_url")]
    public string? FlagUrl { get; set; }

    [JsonPropertyName("flag_svg_content")]
    public string? FlagSvgContent { get; set; }

    public string FlagImagePath => !string.IsNullOrWhiteSpace(Alpha2)
        ? $"/Assets/Flags/{Alpha2.ToLowerInvariant()}.png"
        : string.Empty;

    public string FlagEmoji => ConvertToEmoji(Alpha2);

    public string DisplayName => !string.IsNullOrWhiteSpace(FlagEmoji)
        ? $"{FlagEmoji}  {NameAr} ({PhoneCode})"
        : $"{NameAr} ({PhoneCode})";

    public override string ToString() => NameAr;

    private static string ConvertToEmoji(string? code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 2)
        {
            return string.Empty;
        }

        var upper = code.ToUpperInvariant();
        if (upper[0] < 'A' || upper[0] > 'Z' || upper[1] < 'A' || upper[1] > 'Z')
        {
            return string.Empty;
        }

        var first = char.ConvertFromUtf32(0x1F1E6 + upper[0] - 'A');
        var second = char.ConvertFromUtf32(0x1F1E6 + upper[1] - 'A');
        return first + second;
    }
}