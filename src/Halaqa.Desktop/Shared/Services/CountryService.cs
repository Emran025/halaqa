using System.IO;
using System.Text.Json;
using Halaqa.Desktop.Shared.Data.Models;

namespace Halaqa.Desktop.Shared.Services;

public sealed class CountryService : ICountryService
{
    private readonly List<CountryItem> _countries = new();
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public CountryService()
    {
        LoadCountries();
    }

    public IReadOnlyList<CountryItem> GetAllCountries() => _countries;

    public IReadOnlyList<CountryItem> SearchCountries(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return _countries;
        }

        var trimmed = query.Trim().ToLowerInvariant();
        return _countries.Where(c =>
            c.NameAr.Contains(trimmed, StringComparison.OrdinalIgnoreCase) ||
            c.NameEn.Contains(trimmed, StringComparison.OrdinalIgnoreCase) ||
            c.PhoneCode.Contains(trimmed, StringComparison.OrdinalIgnoreCase) ||
            c.Alpha2.Equals(trimmed, StringComparison.OrdinalIgnoreCase) ||
            c.Alpha3.Equals(trimmed, StringComparison.OrdinalIgnoreCase)
        ).ToList();
    }

    public CountryItem? FindByCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        var trimmed = code.Trim().ToUpperInvariant();
        return _countries.FirstOrDefault(c =>
            c.Alpha2.Equals(trimmed, StringComparison.OrdinalIgnoreCase) ||
            c.Alpha3.Equals(trimmed, StringComparison.OrdinalIgnoreCase) ||
            c.PhoneCode.Equals(code.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public CountryItem? FindByName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var trimmed = name.Trim();
        return _countries.FirstOrDefault(c =>
            c.NameAr.Equals(trimmed, StringComparison.OrdinalIgnoreCase) ||
            c.NameEn.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
    }

    private void LoadCountries()
    {
        try
        {
            var baseDir = AppContext.BaseDirectory;
            var path = Path.Combine(baseDir, "Assets", "Data", "countries.json");
            if (!File.Exists(path))
            {
                path = Path.Combine(Directory.GetCurrentDirectory(), "src", "Halaqa.Desktop", "Assets", "Data", "countries.json");
            }

            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<CountryItem>>(json, JsonOptions);
                if (list != null)
                {
                    _countries.AddRange(list.Where(c => !string.IsNullOrWhiteSpace(c.NameAr)));
                }
            }
        }
        catch
        {
            // Fall back silently
        }
    }
}