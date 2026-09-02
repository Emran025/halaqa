using Halaqa.Desktop.Shared.Data.Models;

namespace Halaqa.Desktop.Shared.Services;

public interface ICountryService
{
    IReadOnlyList<CountryItem> GetAllCountries();
    IReadOnlyList<CountryItem> SearchCountries(string? query);
    CountryItem? FindByCode(string? code);
    CountryItem? FindByName(string? name);
}