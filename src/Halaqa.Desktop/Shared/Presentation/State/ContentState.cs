namespace Halaqa.Desktop.Shared.Presentation.State;

public enum ContentStateKind
{
    Idle,
    Loading,
    Ready,
    Empty,
    Error
}

public enum StatusTone
{
    Neutral,
    Info,
    Success,
    Warning,
    Error
}

public sealed record ContentState(ContentStateKind Kind, string? Message = null)
{
    public static ContentState Idle() => new(ContentStateKind.Idle);
    public static ContentState Loading(string? message = null) => new(ContentStateKind.Loading, message);
    public static ContentState Ready() => new(ContentStateKind.Ready);
    public static ContentState Empty(string message) => new(ContentStateKind.Empty, message);
    public static ContentState Error(string message) => new(ContentStateKind.Error, message);

    public bool IsBlocking => Kind is ContentStateKind.Loading or ContentStateKind.Error;
}
