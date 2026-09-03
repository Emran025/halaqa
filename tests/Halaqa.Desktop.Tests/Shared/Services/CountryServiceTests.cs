using Halaqa.Desktop.Shared.Services;
using Xunit;

namespace Halaqa.Desktop.Tests.Shared.Services;

public sealed class CountryServiceTests
{
    private readonly CountryService _countryService;

    public CountryServiceTests()
    {
        _countryService = new CountryService();
    }

    [Fact]
    public void GetAllCountries_ReturnsNonEmptyList()
    {
        var countries = _countryService.GetAllCountries();
        Assert.NotNull(countries);
        Assert.NotEmpty(countries);
    }

    [Fact]
    public void SearchCountries_FindsByArabicName()
    {
        var results = _countryService.SearchCountries("السعودية");
        Assert.NotEmpty(results);
        Assert.Contains(results, c => c.PhoneCode == "+966");
    }

    [Fact]
    public void SearchCountries_FindsByEnglishName()
    {
        var results = _countryService.SearchCountries("Yemen");
        Assert.NotEmpty(results);
        Assert.Contains(results, c => c.PhoneCode == "+967");
    }

    [Fact]
    public void FindByCode_FindsByAlpha2()
    {
        var country = _countryService.FindByCode("EG");
        Assert.NotNull(country);
        Assert.Equal("+20", country.PhoneCode);
        Assert.Equal("مصر", country.NameAr);
    }
}