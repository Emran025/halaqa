namespace Halaqa.Desktop.Config;

public sealed class ApiOptions
{
    public const string SectionName = "Api";

    public string BaseUrl { get; init; } = string.Empty;
    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(30);
}
