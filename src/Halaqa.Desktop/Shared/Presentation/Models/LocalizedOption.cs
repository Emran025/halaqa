namespace Halaqa.Desktop.Shared.Presentation.Models;

/// <summary>
/// A presentation-only option that keeps the API/domain value separate from its Arabic label.
/// </summary>
public sealed record LocalizedOption<T>(T Value, string Label)
{
    public override string ToString() => Label;
}
